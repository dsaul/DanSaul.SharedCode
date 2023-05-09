namespace DanSaul.SharedCode.Linode
{
	public record DomainRecordsListResponse(
		List<DomainRecordObject> Data,
		int Page,
		int Pages,
		int Results
		);
}
