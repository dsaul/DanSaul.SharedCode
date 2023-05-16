using System.Text.RegularExpressions;
using System.Globalization;
using Serilog;

namespace DanSaul.SharedCode.StandardizedEnvironmentVariables
{
	public static class EnvDatabases
	{
		public static string BILLING_DATABASE_NAME
		{
			get {
				string? str = Environment.GetEnvironmentVariable("BILLING_DATABASE_NAME");
				if (string.IsNullOrWhiteSpace(str)) {
					throw new Exception("BILLING_DATABASE_NAME empty or missing.");
				}
				return str;
			}
		}

		public static string? ASPNETCORE_ENVIRONMENT
		{
			get {
				string? str = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
				if (string.IsNullOrWhiteSpace(str)) {
					Log.Error("ASPNETCORE_ENVIRONMENT empty or missing.");
					return null;
				}
				return str;
			}
		}

		public static string? NPGSQL_CONNECTION_STRING_FILE
		{
			get {
				string? str = Environment.GetEnvironmentVariable("NPGSQL_CONNECTION_STRING_FILE");
				if (string.IsNullOrWhiteSpace(str)) {
					Log.Error("NPGSQL_CONNECTION_STRING_FILE empty or missing.");
					return null;
				}
				return str;
			}

		}

		public static string? NPGSQL_CONNECTION_STRING
		{
			get {
				string? path = NPGSQL_CONNECTION_STRING_FILE;
				if (string.IsNullOrWhiteSpace(path))
					return null;
				string? str = File.ReadAllText(path);
				if (string.IsNullOrWhiteSpace(str))
					return null;

				return str;
			}
		}

		public static string? PGPASSFILE
		{
			get {
				string? str = Environment.GetEnvironmentVariable("PGPASSFILE");
				if (string.IsNullOrWhiteSpace(str)) {
					Log.Error("PGPASSFILE empty or missing.");
					return null;
				}
				return str;
			}
		}

		public static string KBillingDatabaseConnectionString { get { return DatabaseConnectionStringForDB(BILLING_DATABASE_NAME); } }
		public static CultureInfo KDefaultCulture { get { return CultureInfo.CreateSpecificCulture("en-CA"); } }
		public static string DatabaseConnectionStringForDB(string db) {

			if (db == null)
				throw new ArgumentNullException(nameof(db));

			db = db.Trim();
			db = Regex.Replace(db, "[^a-zA-Z0-9_]", "_");
			db = db.ToLower(EnvDatabases.KDefaultCulture);

			return $"{NPGSQL_CONNECTION_STRING}Database={db};";
		}

		public const string kPermissionValueDeniedString = "kPermissionValueDeniedString";


		public static readonly string[] PermissionKeysUsersCanEdit = {

			kPermBillingContactsReadCompany,
			kPermBillingPermissionsBoolReadSelf,
			kPermBillingSessionsReadSelf,
			kPermBillingPermissionsGroupsMembershipsReadSelf,
			kPermBillingContactsAddCompany,
			kPermBillingContactsModifyCompany,
			kPermBillingContactsDeleteCompany,
			kPermBillingCouponCodesReadCompany,
			kPermBillingIndustriesReadCompany,
			kPermBillingInvoicesReadCompany,
			kPermBillingJournalEntriesReadCompany,
			kPermBillingPackagesReadCompany,
			kPermBillingPackagesTypeReadCompany,
			kPermBillingPaymentFrequenciesReadCompany,
			kPermBillingPaymentMethodReadCompany,
			kPermBillingPermissionsBoolReadCompany,
			kPermBillingPermissionsBoolModifyCompany,
			kPermBillingPermissionsBoolDeleteCompany,
			kPermBillingSessionsReadCompany,
			kPermBillingSessionsAddCompany,
			kPermBillingSessionsModifyCompany,
			kPermBillingSessionsDeleteCompany,
			kPermBillingSubscriptionRequestProvisioningStatusCompany,
			kPermBillingCouponCodesReadCompany,
			kPermBillingIndustriesReadCompany,
			kPermBillingInvoicesReadCompany,
			kPermBillingJournalEntriesReadCompany,
			kPermBillingPackagesReadCompany,
			kPermBillingPackagesTypeReadCompany,
			kPermBillingPaymentFrequenciesReadCompany,
			kPermBillingPaymentMethodReadCompany,
			kPermBillingSubscriptionReadCompany,
			kPermBillingCurrencyReadCompany,
			kPermBillingJournalEntriesTypeReadCompany,
			kPermCRMRequestAgentsCompany,
			kPermCRMDeleteAgentsCompany,
			kPermCRMPushAgentsCompany,
			kPermCRMDeleteEmploymentStatusCompany,
			kPermCRMPushEmploymentStatusCompany,
			kPermCRMRequestEmploymentStatusCompany,
			kPermCRMRequestAssignmentsCompany,
			kPermCRMDeleteAssignmentsCompany,
			kPermCRMPushAssignmentsCompany,
			kPermCRMRequestAssignmentsStatusCompany,
			kPermCRMDeleteAssignmentsStatusCompany,
			kPermCRMPushAssignmentsStatusCompany,
			kPermCRMRequestCompaniesCompany,
			kPermCRMDeleteCompaniesCompany,
			kPermCRMPushCompaniesCompany,
			kPermCRMRequestContactsCompany,
			kPermCRMDeleteContactsCompany,
			kPermCRMPushContactsCompany,
			kPermCRMRequestLabourCompany,
			kPermCRMDeleteLabourCompany,
			kPermCRMPushLabourCompany,
			kPermCRMRequestLabourSubtypeExceptionCompany,
			kPermCRMPushLabourSubtypeExceptionCompany,
			kPermCRMDeleteLabourSubtypeExceptionCompany,
			kPermCRMRequestLabourSubtypeHolidaysCompany,
			kPermCRMDeleteLabourSubtypeHolidaysCompany,
			kPermCRMPushLabourSubtypeHolidaysCompany,
			kPermCRMRequestLabourSubtypeNonBillableCompany,
			kPermCRMDeleteLabourSubtypeNonBillableCompany,
			kPermCRMPushLabourSubtypeNonBillableCompany,
			kPermCRMDeleteLabourTypesCompany,
			kPermCRMPushLabourTypesCompany,
			kPermCRMRequestLabourTypesCompany,
			kPermCRMDeleteMaterialsCompany,
			kPermCRMPushMaterialsCompany,
			kPermCRMRequestMaterialsCompany,
			kPermCRMDeleteProductsCompany,
			kPermCRMPushProductsCompany,
			kPermCRMRequestProductsCompany,
			kPermCRMDeleteProjectNotesCompany,
			kPermCRMPushProjectNotesCompany,
			kPermCRMRequestProjectNotesCompany,
			kPermCRMDeleteProjectsCompany,
			kPermCRMPushProjectsCompany,
			kPermCRMRequestProjectsCompany,
			kPermCRMDeleteProjectStatusCompany,
			kPermCRMPushProjectStatusCompany,
			kPermCRMRequestProjectStatusCompany,
			kPermCRMDeleteSettingsDefaultCompany,
			kPermCRMPushSettingsDefaultCompany,
			kPermCRMRequestSettingsDefaultCompany,
			kPermCRMDeleteSettingsUserCompany,
			kPermCRMPushSettingsUserCompany,
			kPermCRMRequestSettingsUserCompany,
			kPermCRMDeleteSkillsCompany,
			kPermCRMPushSkillsCompany,
			kPermCRMRequestSkillsCompany,
			kPermCRMRequestAgentsSelf,
			kPermCRMAgentsDisplayOwn,
			kPermCRMViewDashboardTab,
			kPermCRMViewDashboardBillingTab,
			kPermCRMViewDashboardManagementTab,
			kPermCRMLabourManualEntries,
			kPermCRMNavigationShowAddressBook,
			kPermCRMNavigationShowAllContacts,
			kPermCRMNavigationShowAllCompanies,
			kPermCRMNavigationShowProjects,
			kPermCRMNavigationShowAllProjects,
			kPermCRMNavigationShowAllAssignments,
			kPermCRMNavigationShowAllMaterialEntries,
			kPermCRMNavigationShowProjectDefinitions,
			kPermCRMNavigationShowProductsDefinitions,
			kPermCRMNavigationShowAssignmentStatusDefinitions,
			kPermCRMNavigationShowManHoursDefinitions,
			kPermCRMNavigationShowProjectStatusDefinitions,
			kPermCRMNavigationShowAgents,
			kPermCRMNavigationShowAllAgents,
			kPermCRMNavigationShowAllLabourEntries,
			kPermCRMNavigationShowAgentsDefinitions,
			kPermCRMNavigationShowEmploymentStatusDefinitions,
			kPermCRMNavigationShowLabourExceptionDefinitions,
			kPermCRMNavigationShowLabourHolidaysDefinitions,
			kPermCRMNavigationShowLabourNonBillableDefinitions,
			kPermCRMNavigationShowReports,
			kPermCRMNavigationShowAllReports,
			kPermCRMNavigationShowSettings,
			kPermCRMReportContactsPDF,
			kPermCRMExportContactsCSV,
			kPermCRMReportCompaniesPDF,
			kPermCRMExportCompaniesCSV,
			kPermCRMReportProjectsPDF,
			kPermCRMReportAssignmentsPDF,
			kPermCRMExportAssignmentsCSV,
			kPermCRMReportMaterialsPDF,
			kPermCRMExportMaterialsCSV,
			kPermCRMExportProductDefinitionsCSV,
			kPermCRMExportAssignmentStatusDefinitionsCSV,
			kPermCRMExportManHoursDefinitionsCSV,
			kPermCRMExportProjectStatusDefinitionsCSV,
			kPermCRMReportLabourPDF,
			kPermCRMExportLabourCSV,
			kPermCRMExportEmploymentStatusDefinitionsCSV,
			kPermCRMExportLabourExceptionDefinitionsCSV,
			kPermCRMExportLabourHolidaysDefinitionsCSV,
			kPermCRMExportLabourNonBillableDefinitionsCSV,
			kPermCRMPushLabourSelf,
			kPermCRMViewBillingIndexMergeProjects,
			kPermCRMExportProjectsCSV,
			kPermCRMRequestAssignmentsSelf,
			kPermCRMRequestMaterialsSelf,
			kPermCRMDeleteLabourSelf,
			kPermCRMExportAgentsCSV,
			kPermCRMDeleteEstimatingManHoursCompany,
			kPermCRMPushEstimatingManHoursCompany,
			kPermCRMRequestEstimatingManHoursCompany,
			kPermCRMRequestSettingsDefaultCompany,
			kPermCRMBackupsRunLocal,
			kPermCRMBackupsRunServer,
			kPermCRMPushDIDsCompany,
			kPermCRMDeleteDIDsCompany,
			kPermCRMRequestSkillsCompany,
			kPermCRMDeleteOnCallAutoAttendantsCompany,
			kPermCRMPushOnCallAutoAttendantsCompany,
			kPermCRMRequestOnCallAutoAttendantsCompany,
			kPermCRMDeleteCalendarsCompany,
			kPermCRMPushCalendarsCompany,
			kPermCRMRequestCalendarsCompany,
			kPermCRMDeleteVoicemailsCompany,
			kPermCRMPushVoicemailsCompany,
			kPermCRMRequestVoicemailsCompany,




		};


		public const string kPermBillingCompaniesReadAny = "billing.companies.read-any";
		public const string kPermBillingCompaniesReadCompany = "billing.companies.read-company";
		public const string kPermBillingCompaniesAddAny = "billing.companies.add-any";
		public const string kPermBillingCompaniesModifyAny = "billing.companies.modify-any";
		public const string kPermBillingCompaniesModifyCompany = "billing.companies.modify-company";
		public const string kPermBillingCompaniesDeleteAny = "billing.companies.delete-any";
		public const string kPermBillingCompaniesDeleteCompany = "billing.companies.delete-company";
		public const string kPermBillingCompaniesModifyCompanyPhoneId = "billing.companies.modify-company-phone-id";

		public const string kPermBillingContactsReadAny = "billing.contacts.read-any";
		public const string kPermBillingContactsReadCompany = "billing.contacts.read-company";
		public const string kPermBillingContactsReadSelf = "billing.contacts.read-self";
		public const string kPermBillingContactsAddAny = "billing.contacts.add-any";
		public const string kPermBillingContactsAddCompany = "billing.contacts.add-company";
		public const string kPermBillingContactsModifyAny = "billing.contacts.modify-any";
		public const string kPermBillingContactsModifyCompany = "billing.contacts.modify-company"; // bare minimum
		public const string kPermBillingContactsModifySelf = "billing.contacts.modify-self";
		public const string kPermBillingContactsDeleteAny = "billing.contacts.delete-any";
		public const string kPermBillingContactsDeleteCompany = "billing.contacts.delete-company";
		public const string kPermBillingContactsDeleteSelf = "billing.contacts.delete-self";


		public const string kPermBillingCouponCodesReadAny = "billing.coupon-codes.read-any";
		public const string kPermBillingCouponCodesReadCompany = "billing.coupon-codes.read-company";
		public const string kPermBillingCouponCodesAddAny = "billing.coupon-codes.add-any";
		public const string kPermBillingCouponCodesModifyAny = "billing.coupon-codes.modify-any";
		public const string kPermBillingCouponCodesDeleteAny = "billing.coupon-codes.delete-any";

		public const string kPermBillingCurrencyReadAny = "billing.currency.read-any"; // SuperAdmin
		public const string kPermBillingCurrencyReadCompany = "billing.currency.read-company";
		public const string kPermBillingCurrencyAddAny = "billing.currency.add-any"; // SuperAdmin
		public const string kPermBillingCurrencyModifyAny = "billing.currency.modify-any"; // SuperAdmin
		public const string kPermBillingCurrencyDeleteAny = "billing.currency.delete-any"; // SuperAdmin

		public const string kPermBillingIndustriesReadAny = "billing.industries.read-any"; // SuperAdmin
		public const string kPermBillingIndustriesReadCompany = "billing.industries.read-company";
		public const string kPermBillingIndustriesAddAny = "billing.industries.add-any"; // SuperAdmin
		public const string kPermBillingIndustriesModifyAny = "billing.industries.modify-any"; // SuperAdmin
		public const string kPermBillingIndustriesDeleteAny = "billing.industries.delete-any"; // SuperAdmin

		public const string kPermBillingInvoicesReadAny = "billing.invoices.read-any";
		public const string kPermBillingInvoicesReadCompany = "billing.invoices.read-company";
		public const string kPermBillingInvoicesAddAny = "billing.invoices.add-any";
		public const string kPermBillingInvoicesModifyAny = "billing.invoices.modify-any";
		public const string kPermBillingInvoicesDeleteAny = "billing.invoices.delete-any";

		public const string kPermBillingJournalEntriesReadAny = "billing.journal-entries.read-any";
		public const string kPermBillingJournalEntriesReadCompany = "billing.journal-entries.read-company";
		public const string kPermBillingJournalEntriesAddAny = "billing.journal-entries.add-any";
		public const string kPermBillingJournalEntriesModifyAny = "billing.journal-entries.modify-any";
		public const string kPermBillingJournalEntriesDeleteAny = "billing.journal-entries.delete-any";

		public const string kPermBillingJournalEntriesTypeReadAny = "billing.journal-entries-type.read-any";
		public const string kPermBillingJournalEntriesTypeReadCompany = "billing.journal-entries-type.read-company";
		public const string kPermBillingJournalEntriesTypeAddAny = "billing.journal-entries-type.add-any";
		public const string kPermBillingJournalEntriesTypeModifyAny = "billing.journal-entries-type.modify-any";
		public const string kPermBillingJournalEntriesTypeDeleteAny = "billing.journal-entries-type.delete-any";



		public const string kPermBillingPackagesReadAny = "billing.packages.read-any";
		public const string kPermBillingPackagesReadCompany = "billing.packages.read-company";
		public const string kPermBillingPackagesAddAny = "billing.packages.add-any";
		public const string kPermBillingPackagesModifyAny = "billing.packages.modify-any";
		public const string kPermBillingPackagesDeleteAny = "billing.packages.delete-any";

		public const string kPermBillingPackagesTypeReadAny = "billing.packages-type.read-any";
		public const string kPermBillingPackagesTypeReadCompany = "billing.packages-type.read-company";
		public const string kPermBillingPackagesTypeAddAny = "billing.packages-type.add-any";
		public const string kPermBillingPackagesTypeModifyAny = "billing.packages-type.modify-any";
		public const string kPermBillingPackagesTypeDeleteAny = "billing.packages-type.delete-any";

		public const string kPermBillingPaymentFrequenciesReadAny = "billing.payment-frequencies.read-any";
		public const string kPermBillingPaymentFrequenciesReadCompany = "billing.payment-frequencies.read-company";
		public const string kPermBillingPaymentFrequenciesAddAny = "billing.payment-frequencies.add-any";
		public const string kPermBillingPaymentFrequenciesModifyAny = "billing.payment-frequencies.modify-any";
		public const string kPermBillingPaymentFrequenciesDeleteAny = "billing.payment-frequencies.delete-any";

		public const string kPermBillingPaymentMethodReadAny = "billing.payment-method.read-any";
		public const string kPermBillingPaymentMethodReadCompany = "billing.payment-method.read-company";
		public const string kPermBillingPaymentMethodAddAny = "billing.payment-method.add-any";
		public const string kPermBillingPaymentMethodModifyAny = "billing.payment-method.modify-any";
		public const string kPermBillingPaymentMethodDeleteAny = "billing.payment-method.delete-any";

		public const string kPermBillingPermissionsBoolReadAny = "billing.permissions-bool.read-any";
		public const string kPermBillingPermissionsBoolReadCompany = "billing.permissions-bool.read-company";
		public const string kPermBillingPermissionsBoolReadSelf = "billing.permissions-bool.read-self";
		public const string kPermBillingPermissionsBoolAddAny = "billing.permissions-bool.add-any";
		public const string kPermBillingPermissionsBoolAddCompany = "billing.permissions-bool.add-company";
		public const string kPermBillingPermissionsBoolModifyAny = "billing.permissions-bool.modify-any";
		public const string kPermBillingPermissionsBoolModifyCompany = "billing.permissions-bool.modify-company";
		public const string kPermBillingPermissionsBoolModifySelf = "billing.permissions-bool.modify-self";
		public const string kPermBillingPermissionsBoolDeleteAny = "billing.permissions-bool.delete-any";
		public const string kPermBillingPermissionsBoolDeleteCompany = "billing.permissions-bool.delete-company";
		public const string kPermBillingPermissionsBoolDeleteSelf = "billing.permissions-bool.delete-self";

		public const string kPermBillingPermissionsGroupsReadAny = "billing.permissions-groups.read-any";
		public const string kPermBillingPermissionsGroupsReadCompany = "billing.permissions-groups.read-company";
		public const string kPermBillingPermissionsGroupsAddAny = "billing.permissions-groups.add-any";
		public const string kPermBillingPermissionsGroupsModifyAny = "billing.permissions-groups.modify-any";
		public const string kPermBillingPermissionsGroupsDeleteAny = "billing.permissions-groups.delete-any";

		public const string kPermBillingPermissionsGroupsMembershipsReadAny = "billing.permissions-groups-memberships.read-any";
		public const string kPermBillingPermissionsGroupsMembershipsReadCompany = "billing.permissions-groups-memberships.read-company";
		public const string kPermBillingPermissionsGroupsMembershipsReadSelf = "billing.permissions-groups-memberships.read-self";
		public const string kPermBillingPermissionsGroupsMembershipsAddAny = "billing.permissions-groups-memberships.add-any";
		public const string kPermBillingPermissionsGroupsMembershipsModifyAny = "billing.permissions-groups-memberships.modify-any";
		public const string kPermBillingPermissionsGroupsMembershipsModifyCompany = "billing.permissions-groups-memberships.modify-company";
		public const string kPermBillingPermissionsGroupsMembershipsDeleteAny = "billing.permissions-groups-memberships.delete-any";
		public const string kPermBillingPermissionsGroupsMembershipsDeleteCompany = "billing.permissions-groups-memberships.delete-company";

		public const string kPermBillingSessionsReadAny = "billing.sessions.read-any";
		public const string kPermBillingSessionsReadCompany = "billing.sessions.read-company";
		public const string kPermBillingSessionsReadSelf = "billing.sessions.read-self";
		public const string kPermBillingSessionsAddAny = "billing.sessions.add-any";
		public const string kPermBillingSessionsAddCompany = "billing.sessions.add-company";
		public const string kPermBillingSessionsModifyAny = "billing.sessions.modify-any";
		public const string kPermBillingSessionsModifyCompany = "billing.sessions.modify-company";
		public const string kPermBillingSessionsModifySelf = "billing.sessions.modify-self";
		public const string kPermBillingSessionsDeleteAny = "billing.sessions.delete-any";
		public const string kPermBillingSessionsDeleteCompany = "billing.sessions.delete-company";
		public const string kPermBillingSessionsDeleteSelf = "billing.sessions.delete-self";

		public const string kPermBillingSubscriptionRequestProvisioningStatusAny = "billing.subscription-provisioning-status.request-any";
		public const string kPermBillingSubscriptionRequestProvisioningStatusCompany = "billing.subscription-provisioning-status.request-company";

		public const string kPermBillingSubscriptionReadAny = "billing.subscription.read-any";
		public const string kPermBillingSubscriptionReadCompany = "billing.subscription.read-company";
		public const string kPermBillingSubscriptionAddAny = "billing.subscription.add-any";
		public const string kPermBillingSubscriptionModifyAny = "billing.subscription.modify-any";
		public const string kPermBillingSubscriptionDeleteAny = "billing.subscription.delete-any";

		public const string kPermCRMDeleteAgentsAny = "crm.agents.delete-any";
		public const string kPermCRMDeleteAgentsCompany = "crm.agents.delete-company";
		public const string kPermCRMPushAgentsAny = "crm.agents.push-any";
		public const string kPermCRMPushAgentsCompany = "crm.agents.push-company";
		public const string kPermCRMRequestAgentsAny = "crm.agents.request-any";
		public const string kPermCRMRequestAgentsCompany = "crm.agents.request-company";
		public const string kPermCRMRequestAgentsSelf = "crm.agents.request-self";
		public const string kPermCRMAgentsDisplayOwn = "crm.agents.display-own";

		public const string kPermCRMDeleteEmploymentStatusAny = "crm.employment-status.delete-any";
		public const string kPermCRMDeleteEmploymentStatusCompany = "crm.employment-status.delete-company";
		public const string kPermCRMPushEmploymentStatusAny = "crm.employment-status.push-any";
		public const string kPermCRMPushEmploymentStatusCompany = "crm.employment-status.push-company";
		public const string kPermCRMRequestEmploymentStatusAny = "crm.employment-status.request-any";
		public const string kPermCRMRequestEmploymentStatusCompany = "crm.employment-status.request-company";

		public const string kPermCRMDeleteAssignmentsAny = "crm.assignments.delete-any";
		public const string kPermCRMDeleteAssignmentsCompany = "crm.assignments.delete-company";
		public const string kPermCRMPushAssignmentsAny = "crm.assignments.push-any";
		public const string kPermCRMPushAssignmentsCompany = "crm.assignments.push-company";
		public const string kPermCRMRequestAssignmentsAny = "crm.assignments.request-any";
		public const string kPermCRMRequestAssignmentsCompany = "crm.assignments.request-company";
		public const string kPermCRMRequestAssignmentsSelf = "crm.assignments.request-self";

		public const string kPermCRMDeleteAssignmentsStatusAny = "crm.assignments-status.delete-any";
		public const string kPermCRMDeleteAssignmentsStatusCompany = "crm.assignments-status.delete-company";
		public const string kPermCRMPushAssignmentsStatusAny = "crm.assignments-status.push-any";
		public const string kPermCRMPushAssignmentsStatusCompany = "crm.assignments-status.push-company";
		public const string kPermCRMRequestAssignmentsStatusAny = "crm.assignments-status.request-any";
		public const string kPermCRMRequestAssignmentsStatusCompany = "crm.assignments-status.request-company";

		public const string kPermCRMDeleteCompaniesAny = "crm.companies.delete-any";
		public const string kPermCRMDeleteCompaniesCompany = "crm.companies.delete-company";
		public const string kPermCRMPushCompaniesAny = "crm.companies.push-any";
		public const string kPermCRMPushCompaniesCompany = "crm.companies.push-company";
		public const string kPermCRMRequestCompaniesAny = "crm.companies.request-any";
		public const string kPermCRMRequestCompaniesCompany = "crm.companies.request-company";

		public const string kPermCRMDeleteContactsAny = "crm.contacts.delete-any";
		public const string kPermCRMDeleteContactsCompany = "crm.contacts.delete-company";
		public const string kPermCRMPushContactsAny = "crm.contacts.push-any";
		public const string kPermCRMPushContactsCompany = "crm.contacts.push-company";
		public const string kPermCRMRequestContactsAny = "crm.contacts.request-any";
		public const string kPermCRMRequestContactsCompany = "crm.contacts.request-company";

		public const string kPermCRMDeleteEstimatingManHoursAny = "crm.estimating-man-hours.delete-any";
		public const string kPermCRMDeleteEstimatingManHoursCompany = "crm.estimating-man-hours.delete-company";
		public const string kPermCRMPushEstimatingManHoursAny = "crm.estimating-man-hours.push-any";
		public const string kPermCRMPushEstimatingManHoursCompany = "crm.estimating-man-hours.push-company";
		public const string kPermCRMRequestEstimatingManHoursAny = "crm.estimating-man-hours.request-any";
		public const string kPermCRMRequestEstimatingManHoursCompany = "crm.estimating-man-hours.request-company";

		public const string kPermCRMDeleteLabourAny = "crm.labour.delete-any";
		public const string kPermCRMDeleteLabourCompany = "crm.labour.delete-company";
		public const string kPermCRMDeleteLabourSelf = "crm.labour.delete-self";
		public const string kPermCRMPushLabourAny = "crm.labour.push-any";
		public const string kPermCRMPushLabourCompany = "crm.labour.push-company";
		public const string kPermCRMPushLabourSelf = "crm.labour.push-self";
		public const string kPermCRMRequestLabourAny = "crm.labour.request-any";
		public const string kPermCRMRequestLabourCompany = "crm.labour.request-company";

		public const string kPermCRMDeleteLabourSubtypeExceptionAny = "crm.labour-subtype-exception.delete-any";
		public const string kPermCRMDeleteLabourSubtypeExceptionCompany = "crm.labour-subtype-exception.delete-company";
		public const string kPermCRMPushLabourSubtypeExceptionAny = "crm.labour-subtype-exception.push-any";
		public const string kPermCRMPushLabourSubtypeExceptionCompany = "crm.labour-subtype-exception.push-company";
		public const string kPermCRMRequestLabourSubtypeExceptionAny = "crm.labour-subtype-exception.request-any";
		public const string kPermCRMRequestLabourSubtypeExceptionCompany = "crm.labour-subtype-exception.request-company";

		public const string kPermCRMDeleteLabourSubtypeHolidaysAny = "crm.labour-subtype-holidays.delete-any";
		public const string kPermCRMDeleteLabourSubtypeHolidaysCompany = "crm.labour-subtype-holidays.delete-company";
		public const string kPermCRMPushLabourSubtypeHolidaysAny = "crm.labour-subtype-holidays.push-any";
		public const string kPermCRMPushLabourSubtypeHolidaysCompany = "crm.labour-subtype-holidays.push-company";
		public const string kPermCRMRequestLabourSubtypeHolidaysAny = "crm.labour-subtype-holidays.request-any";
		public const string kPermCRMRequestLabourSubtypeHolidaysCompany = "crm.labour-subtype-holidays.request-company";

		public const string kPermCRMDeleteLabourSubtypeNonBillableAny = "crm.labour-subtype-non-billable.delete-any";
		public const string kPermCRMDeleteLabourSubtypeNonBillableCompany = "crm.labour-subtype-non-billable.delete-company";
		public const string kPermCRMPushLabourSubtypeNonBillableAny = "crm.labour-subtype-non-billable.push-any";
		public const string kPermCRMPushLabourSubtypeNonBillableCompany = "crm.labour-subtype-non-billable.push-company";
		public const string kPermCRMRequestLabourSubtypeNonBillableAny = "crm.labour-subtype-non-billable.request-any";
		public const string kPermCRMRequestLabourSubtypeNonBillableCompany = "crm.labour-subtype-non-billable.request-company";

		public const string kPermCRMDeleteLabourTypesAny = "crm.labour-types.delete-any";
		public const string kPermCRMDeleteLabourTypesCompany = "crm.labour-types.delete-company";
		public const string kPermCRMPushLabourTypesAny = "crm.labour-types.push-any";
		public const string kPermCRMPushLabourTypesCompany = "crm.labour-types.push-company";
		public const string kPermCRMRequestLabourTypesAny = "crm.labour-types.request-any";
		public const string kPermCRMRequestLabourTypesCompany = "crm.labour-types.request-company";

		public const string kPermCRMDeleteMaterialsAny = "crm.materials.delete-any";
		public const string kPermCRMDeleteMaterialsCompany = "crm.materials.delete-company";
		public const string kPermCRMPushMaterialsAny = "crm.materials.push-any";
		public const string kPermCRMPushMaterialsCompany = "crm.materials.push-company";
		public const string kPermCRMRequestMaterialsAny = "crm.materials.request-any";
		public const string kPermCRMRequestMaterialsCompany = "crm.materials.request-company";
		public const string kPermCRMRequestMaterialsSelf = "crm.materials.request-self";

		public const string kPermCRMDeleteProductsAny = "crm.products.delete-any";
		public const string kPermCRMDeleteProductsCompany = "crm.products.delete-company";
		public const string kPermCRMPushProductsAny = "crm.products.push-any";
		public const string kPermCRMPushProductsCompany = "crm.products.push-company";
		public const string kPermCRMRequestProductsAny = "crm.products.request-any";
		public const string kPermCRMRequestProductsCompany = "crm.products.request-company";

		public const string kPermCRMDeleteProjectNotesAny = "crm.project-notes.delete-any";
		public const string kPermCRMDeleteProjectNotesCompany = "crm.project-notes.delete-company";
		public const string kPermCRMPushProjectNotesAny = "crm.project-notes.push-any";
		public const string kPermCRMPushProjectNotesCompany = "crm.project-notes.push-company";
		public const string kPermCRMRequestProjectNotesAny = "crm.project-notes.request-any";
		public const string kPermCRMRequestProjectNotesCompany = "crm.project-notes.request-company";

		public const string kPermCRMDeleteProjectsAny = "crm.projects.delete-any";
		public const string kPermCRMDeleteProjectsCompany = "crm.projects.delete-company";
		public const string kPermCRMPushProjectsAny = "crm.projects.push-any";
		public const string kPermCRMPushProjectsCompany = "crm.projects.push-company";
		public const string kPermCRMRequestProjectsAny = "crm.projects.request-any";
		public const string kPermCRMRequestProjectsCompany = "crm.projects.request-company";

		public const string kPermCRMDeleteProjectStatusAny = "crm.project-status.delete-any";
		public const string kPermCRMDeleteProjectStatusCompany = "crm.project-status.delete-company";
		public const string kPermCRMPushProjectStatusAny = "crm.project-status.push-any";
		public const string kPermCRMPushProjectStatusCompany = "crm.project-status.push-company";
		public const string kPermCRMRequestProjectStatusAny = "crm.project-status.request-any";
		public const string kPermCRMRequestProjectStatusCompany = "crm.project-status.request-company";

		public const string kPermCRMDeleteSettingsDefaultAny = "crm.settings-default.delete-any";
		public const string kPermCRMDeleteSettingsDefaultCompany = "crm.settings-default.delete-company";
		public const string kPermCRMPushSettingsDefaultAny = "crm.settings-default.push-any";
		public const string kPermCRMPushSettingsDefaultCompany = "crm.settings-default.push-company";
		public const string kPermCRMRequestSettingsDefaultAny = "crm.settings-default.request-any";
		public const string kPermCRMRequestSettingsDefaultCompany = "crm.settings-default.request-company";

		public const string kPermCRMDeleteSettingsProvisioningAny = "crm.settings-provisioning.delete-any";
		public const string kPermCRMDeleteSettingsProvisioningCompany = "crm.settings-provisioning.delete-company";
		public const string kPermCRMPushSettingsProvisioningAny = "crm.settings-provisioning.push-any";
		public const string kPermCRMPushSettingsProvisioningCompany = "crm.settings-provisioning.push-company";
		public const string kPermCRMRequestSettingsProvisioningAny = "crm.settings-provisioning.request-any";
		public const string kPermCRMRequestSettingsProvisioningCompany = "crm.settings-provisioning.request-company";

		public const string kPermCRMDeleteSettingsUserAny = "crm.settings-user.delete-any";
		public const string kPermCRMDeleteSettingsUserCompany = "crm.settings-user.delete-company";
		public const string kPermCRMPushSettingsUserAny = "crm.settings-user.push-any";
		public const string kPermCRMPushSettingsUserCompany = "crm.settings-user.push-company";
		public const string kPermCRMRequestSettingsUserAny = "crm.settings-user.request-any";
		public const string kPermCRMRequestSettingsUserCompany = "crm.settings-user.request-company";

		public const string kPermCRMDeleteSkillsAny = "crm.skills.delete-any";
		public const string kPermCRMDeleteSkillsCompany = "crm.skills.delete-company";
		public const string kPermCRMPushSkillsAny = "crm.skills.push-any";
		public const string kPermCRMPushSkillsCompany = "crm.skills.push-company";
		public const string kPermCRMRequestSkillsAny = "crm.skills.request-any";
		public const string kPermCRMRequestSkillsCompany = "crm.skills.request-company";


		public const string kPermCRMDeleteDIDsAny = "crm.dids.delete-any";
		public const string kPermCRMDeleteDIDsCompany = "crm.dids.delete-company";
		public const string kPermCRMPushDIDsAny = "crm.dids.push-any";
		public const string kPermCRMPushDIDsCompany = "crm.dids.push-company";
		public const string kPermCRMRequestDIDsAny = "crm.dids.request-any";
		public const string kPermCRMRequestDIDsCompany = "crm.dids.request-company";


		public const string kPermCRMDeleteVoicemailsAny = "crm.voicemails.delete-any";
		public const string kPermCRMDeleteVoicemailsCompany = "crm.voicemails.delete-company";
		public const string kPermCRMPushVoicemailsAny = "crm.voicemails.push-any";
		public const string kPermCRMPushVoicemailsCompany = "crm.voicemails.push-company";
		public const string kPermCRMRequestVoicemailsAny = "crm.voicemails.request-any";
		public const string kPermCRMRequestVoicemailsCompany = "crm.voicemails.request-company";

		public const string kPermCRMDeleteRecordingsAny = "crm.recordings.delete-any";
		public const string kPermCRMDeleteRecordingsCompany = "crm.recordings.delete-company";
		public const string kPermCRMPushRecordingsAny = "crm.recordings.push-any";
		public const string kPermCRMPushRecordingsCompany = "crm.recordings.push-company";
		public const string kPermCRMRequestRecordingsAny = "crm.recordings.request-any";
		public const string kPermCRMRequestRecordingsCompany = "crm.recordings.request-company";





		public const string kPermCRMDeleteOnCallAutoAttendantsAny = "crm.on-call-auto-attendants.delete-any";
		public const string kPermCRMDeleteOnCallAutoAttendantsCompany = "crm.on-call-auto-attendants.delete-company";
		public const string kPermCRMPushOnCallAutoAttendantsAny = "crm.on-call-auto-attendants.push-any";
		public const string kPermCRMPushOnCallAutoAttendantsCompany = "crm.on-call-auto-attendants.push-company";
		public const string kPermCRMRequestOnCallAutoAttendantsAny = "crm.on-call-auto-attendants.request-any";
		public const string kPermCRMRequestOnCallAutoAttendantsCompany = "crm.on-call-auto-attendants.request-company";

		public const string kPermCRMDeleteCalendarsAny = "crm.calendars.delete-any";
		public const string kPermCRMDeleteCalendarsCompany = "crm.calendars.delete-company";
		public const string kPermCRMPushCalendarsAny = "crm.calendars.push-any";
		public const string kPermCRMPushCalendarsCompany = "crm.calendars.push-company";
		public const string kPermCRMRequestCalendarsAny = "crm.calendars.request-any";
		public const string kPermCRMRequestCalendarsCompany = "crm.calendars.request-company";


		public const string kPermCRMViewDashboardTab = "crm.view.dashboard.dispatch-tab";
		public const string kPermCRMViewDashboardBillingTab = "crm.view.dashboard.billing-tab";
		public const string kPermCRMViewDashboardManagementTab = "crm.view.dashboard.management-tab";
		public const string kPermCRMLabourManualEntries = "crm.labour.manual-entries";
		public const string kPermCRMNavigationShowAddressBook = "crm.navigation.show.address-book";
		public const string kPermCRMNavigationShowAllContacts = "crm.navigation.show.all-contacts";
		public const string kPermCRMNavigationShowAllCompanies = "crm.navigation.show.all-companies";
		public const string kPermCRMNavigationShowProjects = "crm.navigation.show.projects";
		public const string kPermCRMNavigationShowAllProjects = "crm.navigation.show.all-projects";
		public const string kPermCRMNavigationShowAllAssignments = "crm.navigation.show.all-assignments";
		public const string kPermCRMNavigationShowAllMaterialEntries = "crm.navigation.show.all-material-entries";
		public const string kPermCRMNavigationShowProjectDefinitions = "crm.navigation.show.project-definitions";
		public const string kPermCRMNavigationShowProductsDefinitions = "crm.navigation.show.product-definitions";
		public const string kPermCRMNavigationShowAssignmentStatusDefinitions = "crm.navigation.show.assignment-status-definitions";
		public const string kPermCRMNavigationShowManHoursDefinitions = "crm.navigation.show.man-hours-definitions";
		public const string kPermCRMNavigationShowProjectStatusDefinitions = "crm.navigation.show.project-status-definitions";
		public const string kPermCRMNavigationShowAgents = "crm.navigation.show.agents";
		public const string kPermCRMNavigationShowAllAgents = "crm.navigation.show.all-agents";
		public const string kPermCRMNavigationShowAllLabourEntries = "crm.navigation.show.all-labour-entries";
		public const string kPermCRMNavigationShowAgentsDefinitions = "crm.navigation.show.agents-definitions";
		public const string kPermCRMNavigationShowEmploymentStatusDefinitions = "crm.navigation.show.employment-status-definitions";
		public const string kPermCRMNavigationShowLabourExceptionDefinitions = "crm.navigation.show.labour-exception-definitions";
		public const string kPermCRMNavigationShowLabourHolidaysDefinitions = "crm.navigation.show.labour-holidays-definitions";
		public const string kPermCRMNavigationShowLabourNonBillableDefinitions = "crm.navigation.show.labour-non-billable-definitions";
		public const string kPermCRMNavigationShowReports = "crm.navigation.show.reports";
		public const string kPermCRMNavigationShowAllReports = "crm.navigation.show.all-reports";
		public const string kPermCRMNavigationShowSettings = "crm.navigation.show.settings";


		public const string kPermCRMExportAgentsCSV = "crm.export.agents-csv";
		public const string kPermCRMReportContactsPDF = "crm.report.contacts-pdf";
		public const string kPermCRMExportContactsCSV = "crm.export.contacts-csv";
		public const string kPermCRMReportCompaniesPDF = "crm.report.companies-pdf";
		public const string kPermCRMExportCompaniesCSV = "crm.export.companies-csv";
		public const string kPermCRMReportProjectsPDF = "crm.report.projects-pdf";
		public const string kPermCRMExportProjectsCSV = "crm.export.projects-csv";
		public const string kPermCRMReportAssignmentsPDF = "crm.report.assignments-pdf";
		public const string kPermCRMExportAssignmentsCSV = "crm.export.assignments-csv";
		public const string kPermCRMReportMaterialsPDF = "crm.report.materials-pdf";
		public const string kPermCRMExportMaterialsCSV = "crm.export.materials-csv";
		public const string kPermCRMReportOnCallResponder30DayPDF = "crm.report.on-call-responder-30-days-pdf";
		public const string kPermCRMExportProductDefinitionsCSV = "crm.export.product-definitions-csv";
		public const string kPermCRMExportAssignmentStatusDefinitionsCSV = "crm.export.assignment-status-definitions-csv";
		public const string kPermCRMExportManHoursDefinitionsCSV = "crm.export.man-hours-definitions-csv";
		public const string kPermCRMExportProjectStatusDefinitionsCSV = "crm.export.project-status-definitions-csv";
		public const string kPermCRMReportLabourPDF = "crm.report.labour-pdf";
		public const string kPermCRMExportLabourCSV = "crm.export.labour-csv";
		public const string kPermCRMExportEmploymentStatusDefinitionsCSV = "crm.export.employment-status-definitions-csv";
		public const string kPermCRMExportLabourExceptionDefinitionsCSV = "crm.export.labour-exception-definitions-csv";
		public const string kPermCRMExportLabourHolidaysDefinitionsCSV = "crm.export.labour-holidays-definitions-csv";
		public const string kPermCRMExportLabourNonBillableDefinitionsCSV = "crm.export.labour-non-billable-definitions-csv";
		public const string kPermCRMViewBillingIndexMergeProjects = "crm.view.billing-index.merge-projects";

		public const string kPermCRMBackupsRunLocal = "crm.backups.run-local";
		public const string kPermCRMBackupsRunServer = "crm.backups.run-server";

		public const string kPermBillingCanMakeOneTimeCompanyCreditCardPayments = "billing.credit-card-payments.one-time";
		public const string kPermBillingCanSetupPreAuthorizedCreditCardPayments = "billing.credit-card-payments.setup-pre-authorized";
	}
}
