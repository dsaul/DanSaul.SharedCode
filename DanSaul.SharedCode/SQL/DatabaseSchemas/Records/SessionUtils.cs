using DanSaul.SharedCode.SignalR;
using DanSaul.SharedCode.StandardizedEnvironmentVariables;
using Microsoft.AspNetCore.SignalR;
using Npgsql;

namespace SharedCode.DatabaseSchemas
{
	public static class SessionUtils
	{
		public static void CreateSession(
			Hub? hub,
			IdempotencyResponse response,
			string companyAbbreviation,
			string contactEMail,
			string contactPassword,
			string? tzIANA,
			out string? billingConnectionString,
			out NpgsqlConnection? billingConnection,
			out BillingSessions? session,
			out BillingContacts? billingContact,
			out BillingCompanies? billingCompany,
			out string? dpDBName,
			out string? dpDBConnectionString,
			out NpgsqlConnection? dpDBConnection
			)
		{
			if (null == hub)
				throw new ArgumentNullException(nameof(hub));
			if (null == response)
				throw new ArgumentNullException(nameof(response));


			if (string.IsNullOrWhiteSpace(companyAbbreviation))
			{
				response.ErrorMessage = "Missing company abbreviation.";
				response.IsError = true;

				billingConnectionString = null;
				billingConnection = null;
				session = null;
				billingContact = null;
				billingCompany = null;
				dpDBName = null;
				dpDBConnectionString = null;
				dpDBConnection = null;
				return;
			}
			if (string.IsNullOrWhiteSpace(contactEMail))
			{
				response.ErrorMessage = "Missing email.";
				response.IsError = true;

				billingConnectionString = null;
				billingConnection = null;
				session = null;
				billingContact = null;
				billingCompany = null;
				dpDBName = null;
				dpDBConnectionString = null;
				dpDBConnection = null;
				return;
			}
			if (string.IsNullOrWhiteSpace(contactPassword))
			{
				response.ErrorMessage = "Missing password.";
				response.IsError = true;

				billingConnectionString = null;
				billingConnection = null;
				session = null;
				billingContact = null;
				billingCompany = null;
				dpDBName = null;
				dpDBConnectionString = null;
				dpDBConnection = null;
				return;
			}

			billingConnectionString = EnvDatabases.DatabaseConnectionStringForDB(EnvDatabases.BILLING_DATABASE_NAME);
			if (string.IsNullOrWhiteSpace(billingConnectionString))
			{
				response.IsError = true;
				response.ErrorMessage = "Couldn't get connection information for the billing system.";

				billingConnection = null;
				session = null;
				billingContact = null;
				billingCompany = null;
				dpDBName = null;
				dpDBConnectionString = null;
				dpDBConnection = null;
				return;
			}


			billingConnection = new NpgsqlConnection(billingConnectionString);
			if (null == billingConnection)
			{
				response.IsError = true;
				response.ErrorMessage = "Couldn't open a connection to the billing system. #1";

				session = null;
				billingContact = null;
				billingCompany = null;
				dpDBName = null;
				dpDBConnectionString = null;
				dpDBConnection = null;
				return;
			}

			billingConnection.Open();
			if (billingConnection.State != System.Data.ConnectionState.Open)
			{
				response.IsError = true;
				response.ErrorMessage = "Couldn't open a connection to the billing system. #2";

				session = null;
				billingContact = null;
				billingCompany = null;
				dpDBName = null;
				dpDBConnectionString = null;
				dpDBConnection = null;
				return;
			}




			Dictionary<Guid, BillingContacts> results = BillingContacts.ForEMailAndAbbreviation(billingConnection, contactEMail, companyAbbreviation);
			if (null == results || results.Count == 0)
			{
				response.ErrorMessage = "Not authorized. #1";
				response.IsError = true;
				session = null;
				billingContact = null;
				billingCompany = null;
				dpDBName = null;
				dpDBConnectionString = null;
				dpDBConnection = null;
				return;
			}


			billingContact = null;
			session = null;
			foreach (KeyValuePair<Guid, BillingContacts> kvp in results)
			{
				if (null == kvp.Value || null == kvp.Value.Uuid)
					continue;


				if (BCrypt.Net.BCrypt.Verify(contactPassword, kvp.Value.PasswordHash))
				{
					billingContact = kvp.Value;

					// Create session
					Guid id = Guid.NewGuid();
					session = new BillingSessions(
						Uuid: id,
						ContactId: kvp.Value.Uuid.Value,
						AgentDescription: "APIHub",
						IpAddress: "",
						Json: "{}",
						CreatedUtc: DateTime.UtcNow,
						LastAccessUtc: DateTime.UtcNow
						);
					BillingSessions.Upsert(billingConnection, new Dictionary<Guid, BillingSessions>
					{
						{ id, session }
					}, out _, out _);
					break;
				}
			}

			if (null == billingContact || null == session)
			{
				response.ErrorMessage = "Not authorized. #2";
				response.IsError = true;

				billingCompany = null;
				dpDBName = null;
				dpDBConnectionString = null;
				dpDBConnection = null;
				return;
			}

			// Add connection to group.
			if (null != hub)
			{
				if (null != billingContact && null != billingContact.Uuid)
					hub.Groups.AddToGroupAsync(hub.Context.ConnectionId, BillingContacts.UserGroupNameForBillingContact(billingContact));
				if (null != billingContact && null != billingContact.CompanyId)
					hub.Groups.AddToGroupAsync(hub.Context.ConnectionId, BillingContacts.CompanyGroupNameForBillingContact(billingContact));
			}

			// Packages that allow dp provisioning.
			Dictionary<Guid, BillingPackages> dpPackages = BillingPackages.AllThatProvisionDP(billingConnection);
			if (dpPackages == null || dpPackages.Count == 0)
			{
				response.IsError = true;
				response.ErrorMessage = "Cannot find the DP packages.";

				billingCompany = null;
				dpDBName = null;
				dpDBConnectionString = null;
				dpDBConnection = null;
				return;
			}
			IEnumerable<Guid> dpPackagesIds = dpPackages.Keys;

			if (null == billingContact)
			{
				response.IsError = true;
				response.ErrorMessage = "Can't find billing contact.";
				billingCompany = null;
				dpDBName = null;
				dpDBConnectionString = null;
				dpDBConnection = null;
				return;
			}



			Guid? billingCompanyId = billingContact.CompanyId;
			if (null == billingCompanyId)
			{
				response.IsError = true;
				response.ErrorMessage = "Contact doesn't have a company assigned to it.";

				billingCompany = null;
				dpDBName = null;
				dpDBConnectionString = null;
				dpDBConnection = null;
				return;
			}

			Dictionary<Guid, BillingCompanies> billingCompanies = BillingCompanies.ForIds(billingConnection, billingCompanyId.Value);
			if (billingCompanies == null || billingCompanies.Count == 0)
			{
				response.IsError = true;
				response.ErrorMessage = "Cannot find the billing company for the session. #1";

				billingCompany = null;
				dpDBName = null;
				dpDBConnectionString = null;
				dpDBConnection = null;
				return;
			}

			billingCompany = billingCompanies.First().Value;
			if (billingContact == null)
			{
				response.IsError = true;
				response.ErrorMessage = "Cannot find the billing contact for the session. #2";
				dpDBName = null;
				dpDBConnectionString = null;
				dpDBConnection = null;
				return;
			}

			Dictionary<Guid, BillingSubscriptions> subscriptions = BillingSubscriptions.ForCompanyId(billingConnection, billingCompanyId.Value);
			Dictionary<Guid, BillingSubscriptions> subscriptionsFiltered = new Dictionary<Guid, BillingSubscriptions>();
			foreach (KeyValuePair<Guid, BillingSubscriptions> subscription in subscriptions)
			{
				if (null == subscription.Value || null == subscription.Value.PackageId)
					continue;
				if (string.IsNullOrWhiteSpace(subscription.Value.ProvisionedDatabaseName))
					continue;
				if (!dpPackagesIds.Contains(subscription.Value.PackageId.Value))
					continue;

				subscriptionsFiltered.Add(subscription.Key, subscription.Value);
			}

			if (subscriptionsFiltered.Count == 0)
			{
				response.IsError = true;
				response.ErrorMessage = "Can't find active subscription. #1";

				dpDBName = null;
				dpDBConnectionString = null;
				dpDBConnection = null;
				return;
			}


			BillingSubscriptions sub = subscriptionsFiltered.First().Value;
			if (sub == null)
			{
				response.IsError = true;
				response.ErrorMessage = "Can't find active subscription. #2";

				dpDBName = null;
				dpDBConnectionString = null;
				dpDBConnection = null;
				return;
			}

			dpDBName = sub.ProvisionedDatabaseName;
			if (string.IsNullOrWhiteSpace(dpDBName))
			{
				response.IsError = true;
				response.ErrorMessage = "Can't find database name.";
				dpDBConnectionString = null;
				dpDBConnection = null;
				return;
			}

			dpDBConnectionString = EnvDatabases.DatabaseConnectionStringForDB(dpDBName);
			if (string.IsNullOrWhiteSpace(dpDBConnectionString))
			{
				response.IsError = true;
				response.ErrorMessage = "Cannot get database connection string.";
				dpDBConnection = null;
				return;
			}


			dpDBConnection = new NpgsqlConnection(dpDBConnectionString);
			if (null == billingConnection)
			{
				response.IsError = true;
				response.ErrorMessage = "null == dbConnection";
				return;
			}
			dpDBConnection.Open();
			if (dpDBConnection.State != System.Data.ConnectionState.Open)
			{
				response.IsError = true;
				response.ErrorMessage = "dbConnection.State != System.Data.dbConnection.Open";
				return;
			}
		}






		public static void GetSessionInformation(
			Hub? hub,
			IdempotencyResponse response,
			Guid? sessionId,
			out string? billingConnectionString,
			out NpgsqlConnection? billingConnection,
			out BillingSessions? session,
			out BillingContacts? billingContact,
			out BillingCompanies? billingCompany,
			out string? dpDBName,
			out string? dpDBConnectionString,
			out NpgsqlConnection? dpDBConnection,
			bool skipDispatchPulse = false
			)
		{
			if (response == null)
			{
				billingConnectionString = null;
				billingConnection = null;
				session = null;
				billingContact = null;
				billingCompany = null;
				dpDBName = null;
				dpDBConnectionString = null;
				dpDBConnection = null;
				return;
			}


			if (sessionId == null)
			{
				response.IsError = true;
				response.ErrorMessage = "No session provided.";

				billingConnectionString = null;
				billingConnection = null;
				session = null;
				billingContact = null;
				billingCompany = null;
				dpDBName = null;
				dpDBConnectionString = null;
				dpDBConnection = null;
				return;
			}

			billingConnectionString = EnvDatabases.DatabaseConnectionStringForDB(EnvDatabases.BILLING_DATABASE_NAME);
			if (string.IsNullOrWhiteSpace(billingConnectionString))
			{
				response.IsError = true;
				response.ErrorMessage = "Couldn't get connection information for the billing system.";

				billingConnection = null;
				session = null;
				billingContact = null;
				billingCompany = null;
				dpDBName = null;
				dpDBConnectionString = null;
				dpDBConnection = null;
				return;
			}


			billingConnection = new NpgsqlConnection(billingConnectionString);
			if (null == billingConnection)
			{
				response.IsError = true;
				response.ErrorMessage = "Couldn't open a connection to the billing system. #1";

				session = null;
				billingContact = null;
				billingCompany = null;
				dpDBName = null;
				dpDBConnectionString = null;
				dpDBConnection = null;
				return;
			}

			billingConnection.Open();
			if (billingConnection.State != System.Data.ConnectionState.Open)
			{
				response.IsError = true;
				response.ErrorMessage = "Couldn't open a connection to the billing system. #2";

				session = null;
				billingContact = null;
				billingCompany = null;
				dpDBName = null;
				dpDBConnectionString = null;
				dpDBConnection = null;
				return;
			}

			Dictionary<Guid, BillingSessions> sessions = BillingSessions.ForId(billingConnection, sessionId.Value);
			if (sessions == null || sessions.Count == 0)
			{
				response.IsError = true;
				response.ErrorMessage = "Can't find session. #1";

				session = null;
				billingContact = null;
				billingCompany = null;
				dpDBName = null;
				dpDBConnectionString = null;
				dpDBConnection = null;
				return;
			}

			session = sessions.First().Value;
			if (null == session)
			{
				response.IsError = true;
				response.ErrorMessage = "Can't find session. #2";

				billingContact = null;
				billingCompany = null;
				dpDBName = null;
				dpDBConnectionString = null;
				dpDBConnection = null;
				return;
			}

			if (null == session.ContactId)
			{
				response.IsError = true;
				response.ErrorMessage = "No billing contact id.";

				billingContact = null;
				billingCompany = null;
				dpDBName = null;
				dpDBConnectionString = null;
				dpDBConnection = null;
				return;
			}

			Guid billingContactId = session.ContactId.Value;
			if (billingContactId == Guid.Empty)
			{
				response.IsError = true;
				response.ErrorMessage = "No billing contact id on session.";

				billingContact = null;
				billingCompany = null;
				dpDBName = null;
				dpDBConnectionString = null;
				dpDBConnection = null;
				return;
			}

			Dictionary<Guid, BillingContacts> billingContacts = BillingContacts.ForId(billingConnection, billingContactId);
			if (billingContacts == null || billingContacts.Count == 0)
			{
				response.IsError = true;
				response.ErrorMessage = "Cannot find the billing contact for the session. #1";

				billingContact = null;
				billingCompany = null;
				dpDBName = null;
				dpDBConnectionString = null;
				dpDBConnection = null;
				return;
			}

			billingContact = billingContacts.First().Value;
			if (billingContact == null)
			{
				response.IsError = true;
				response.ErrorMessage = "Cannot find the billing contact for the session. #2";

				billingCompany = null;
				dpDBName = null;
				dpDBConnectionString = null;
				dpDBConnection = null;
				return;
			}

			// Add connection to group.
			if (null != hub)
			{
				if (null != billingContact && null != billingContact.Uuid)
					hub.Groups.AddToGroupAsync(hub.Context.ConnectionId, BillingContacts.UserGroupNameForBillingContact(billingContact));
				if (null != billingContact && null != billingContact.CompanyId)
					hub.Groups.AddToGroupAsync(hub.Context.ConnectionId, BillingContacts.CompanyGroupNameForBillingContact(billingContact));
			}



			// Packages that allow dp provisioning.
			Dictionary<Guid, BillingPackages> dpPackages = BillingPackages.AllThatProvisionDP(billingConnection);
			if (dpPackages == null || dpPackages.Count == 0)
			{
				response.IsError = true;
				response.ErrorMessage = "Cannot find the DP packages.";

				billingCompany = null;
				dpDBName = null;
				dpDBConnectionString = null;
				dpDBConnection = null;
				return;
			}
			IEnumerable<Guid> dpPackagesIds = dpPackages.Keys;

			if (billingContact == null)
			{
				response.IsError = true;
				response.ErrorMessage = "Cannot find the billing contact for the session. #3";

				billingCompany = null;
				dpDBName = null;
				dpDBConnectionString = null;
				dpDBConnection = null;
				return;
			}

			Guid? billingCompanyId = billingContact.CompanyId;
			if (null == billingCompanyId)
			{
				response.IsError = true;
				response.ErrorMessage = "Contact doesn't have a company assigned to it.";

				billingCompany = null;
				dpDBName = null;
				dpDBConnectionString = null;
				dpDBConnection = null;
				return;
			}

			Dictionary<Guid, BillingCompanies> billingCompanies = BillingCompanies.ForIds(billingConnection, billingCompanyId.Value);
			if (billingCompanies == null || billingCompanies.Count == 0)
			{
				response.IsError = true;
				response.ErrorMessage = "Cannot find the billing company for the session. #1";

				billingCompany = null;
				dpDBName = null;
				dpDBConnectionString = null;
				dpDBConnection = null;
				return;
			}

			billingCompany = billingCompanies.First().Value;
			if (billingContact == null)
			{
				response.IsError = true;
				response.ErrorMessage = "Cannot find the billing contact for the session. #2";
				dpDBName = null;
				dpDBConnectionString = null;
				dpDBConnection = null;
				return;
			}

			if (false == skipDispatchPulse)
			{
				Dictionary<Guid, BillingSubscriptions> subscriptions = BillingSubscriptions.ForCompanyId(billingConnection, billingCompanyId.Value);
				Dictionary<Guid, BillingSubscriptions> subscriptionsFiltered = new Dictionary<Guid, BillingSubscriptions>();
				foreach (KeyValuePair<Guid, BillingSubscriptions> subscription in subscriptions)
				{
					if (null == subscription.Value || null == subscription.Value.PackageId)
						continue;
					if (string.IsNullOrWhiteSpace(subscription.Value.ProvisionedDatabaseName))
						continue;
					if (!dpPackagesIds.Contains(subscription.Value.PackageId.Value))
						continue;

					subscriptionsFiltered.Add(subscription.Key, subscription.Value);
				}

				if (subscriptionsFiltered.Count == 0)
				{
					response.IsError = true;
					response.ErrorMessage = "Can't find active subscription. #1";

					dpDBName = null;
					dpDBConnectionString = null;
					dpDBConnection = null;
					return;
				}

				BillingSubscriptions sub = subscriptionsFiltered.First().Value;
				if (sub == null)
				{
					response.IsError = true;
					response.ErrorMessage = "Can't find active subscription. #2";

					dpDBName = null;
					dpDBConnectionString = null;
					dpDBConnection = null;
					return;
				}

				dpDBName = sub.ProvisionedDatabaseName;
				if (string.IsNullOrWhiteSpace(dpDBName))
				{
					response.IsError = true;
					response.ErrorMessage = "Can't find database name.";
					dpDBConnectionString = null;
					dpDBConnection = null;
					return;
				}

				dpDBConnectionString = EnvDatabases.DatabaseConnectionStringForDB(dpDBName);
				if (string.IsNullOrWhiteSpace(dpDBConnectionString))
				{
					response.IsError = true;
					response.ErrorMessage = "Cannot get database connection string.";
					dpDBConnection = null;
					return;
				}

				dpDBConnection = new NpgsqlConnection(dpDBConnectionString);
				if (null == billingConnection)
				{
					response.IsError = true;
					response.ErrorMessage = "null == dbConnection";
					return;
				}
				dpDBConnection.Open();
				if (dpDBConnection.State != System.Data.ConnectionState.Open)
				{
					response.IsError = true;
					response.ErrorMessage = "dbConnection.State != System.Data.dbConnection.Open";
					return;
				}
			}
			else
			{
				dpDBConnection = null;
				dpDBConnectionString = null;
				dpDBName = null;
			}
			


			

			


			
		}
	}
}
