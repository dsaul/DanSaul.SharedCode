namespace SharedCode
{
	public record DomainListResponse (
		List<DomainObject> Data,
		int Page,
		int Pages,
		int Results
		);
}
