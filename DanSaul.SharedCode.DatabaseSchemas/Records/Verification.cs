using Npgsql;

namespace SharedCode.DatabaseSchemas
{
	public static class Verification
	{

		public static void VerifyPDFLaTeXDatabase(NpgsqlConnection dpDB, bool insertDefaultContents = false) {
			//PDFLaTeXTask.VerifyRepairTable(dpDB, insertDefaultContents);
		}

		public static void VerifyBillingDatabase(NpgsqlConnection dpDB, bool insertDefaultContents = false) {

			Guid? billingCompanyId;
			Guid? billingContactId;
			Guid? billingPackageCommunityEditionId;

			BillingCompanies.VerifyRepairTable(dpDB, out billingCompanyId, insertDefaultContents);
			BillingContacts.VerifyRepairTable(dpDB, out billingContactId, insertDefaultContents, billingCompanyId);
			BillingCouponCodes.VerifyRepairTable(dpDB, insertDefaultContents);
			BillingCurrency.VerifyRepairTable(dpDB, insertDefaultContents);
			BillingIndustries.VerifyRepairTable(dpDB, insertDefaultContents);
			BillingInvoices.VerifyRepairTable(dpDB, insertDefaultContents);
			BillingJournalEntriesType.VerifyRepairTable(dpDB, insertDefaultContents);
			BillingJournalEntries.VerifyRepairTable(dpDB, insertDefaultContents);
			BillingPackagesType.VerifyRepairTable(dpDB, insertDefaultContents);
			BillingPackages.VerifyRepairTable(dpDB, out billingPackageCommunityEditionId, insertDefaultContents);
			BillingPaymentFrequencies.VerifyRepairTable(dpDB, insertDefaultContents);
			BillingPaymentMethod.VerifyRepairTable(dpDB, insertDefaultContents);
			BillingPermissionsGroups.VerifyRepairTable(dpDB, insertDefaultContents);
			BillingPermissionsBool.VerifyRepairTable(dpDB, insertDefaultContents);
			BillingPermissionsGroupsMemberships.VerifyRepairTable(dpDB, insertDefaultContents);
			BillingSessions.VerifyRepairTable(dpDB, insertDefaultContents);
			BillingSubscriptions.VerifyRepairTable(dpDB, insertDefaultContents);
			BillingSubscriptionsProvisioningStatus.VerifyRepairTable(dpDB, insertDefaultContents);
			RegisteredPhoneNumbers.VerifyRepairTable(dpDB, insertDefaultContents);
			UtilityIpToCountry.VerifyRepairTable(dpDB, insertDefaultContents);
		}


		public static void VerifyJobRunnerDatabase(NpgsqlConnection dpDB, bool insertDefaultContents = false) {
			JobRunnerJob.VerifyRepairTable(dpDB, insertDefaultContents);
			ScheduledTasks.VerifyRepairTable(dpDB, insertDefaultContents);
		}


		public static void VerifyDPClientDatabase(NpgsqlConnection dpDB, bool insertDefaultContents = false) {
			Agents.VerifyRepairTable(dpDB, insertDefaultContents);
			AgentsEmploymentStatus.VerifyRepairTable(dpDB, insertDefaultContents);
			AssignmentStatus.VerifyRepairTable(dpDB, insertDefaultContents);
			Assignments.VerifyRepairTable(dpDB, insertDefaultContents);
			Calendars.VerifyRepairTable(dpDB, insertDefaultContents);
			Companies.VerifyRepairTable(dpDB, insertDefaultContents);
			Contacts.VerifyRepairTable(dpDB, insertDefaultContents);
			DIDs.VerifyRepairTable(dpDB, insertDefaultContents);
			EstimatingManHours.VerifyRepairTable(dpDB, insertDefaultContents);
			Labour.VerifyRepairTable(dpDB, insertDefaultContents);
			LabourSubtypeException.VerifyRepairTable(dpDB, insertDefaultContents);
			LabourSubtypeHolidays.VerifyRepairTable(dpDB, insertDefaultContents);
			LabourSubtypeNonBillable.VerifyRepairTable(dpDB, insertDefaultContents);
			LabourTypes.VerifyRepairTable(dpDB, insertDefaultContents);
			Materials.VerifyRepairTable(dpDB, insertDefaultContents);
			Products.VerifyRepairTable(dpDB, insertDefaultContents);
			ProjectNotes.VerifyRepairTable(dpDB, insertDefaultContents);
			ProjectStatus.VerifyRepairTable(dpDB, insertDefaultContents);
			Projects.VerifyRepairTable(dpDB, insertDefaultContents);
			SettingsDefault.VerifyRepairTable(dpDB, insertDefaultContents);
			SettingsProvisioning.VerifyRepairTable(dpDB, insertDefaultContents);
			SettingsUser.VerifyRepairTable(dpDB, insertDefaultContents);
			Skills.VerifyRepairTable(dpDB, insertDefaultContents);
			OnCallAutoAttendants.VerifyRepairTable(dpDB, insertDefaultContents);
			Voicemails.VerifyRepairTable(dpDB, insertDefaultContents);
			Recordings.VerifyRepairTable(dpDB, insertDefaultContents);
		}
	}
}
