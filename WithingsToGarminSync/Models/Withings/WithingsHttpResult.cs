namespace WithingsToGarminSync.Models.Withings;

public class WithingsHttpResult<T>
{
	public bool IsSuccessful { get; set; }
	public T? Data { get; set; }
	public string? Content { get; set; }
}
