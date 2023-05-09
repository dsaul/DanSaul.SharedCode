using Newtonsoft.Json.Linq;

namespace DanSaul.SharedCode.Extensions.JsonDotNet
{
	public static class JToken_NullExtensions
	{
		/// <summary>
		/// Extension of JToken Value<T> method. Used to avoid null exceptions with value types when parsing JToken.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="token"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public static T ValueOrDefault<T>(this JToken? token, object key) where T : struct {

			if (token == null)
				return default;

			return ValueOrDefault<T>(token[key]);
		}

		/// <summary>
		/// Extension of JToken Value<T> method.  Used to avoid null exceptions with value types when parsing JToken.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="token"></param>
		/// <returns></returns>
		public static T ValueOrDefault<T>(this JToken? token) where T : struct {
			if (token == null)
				return default;

			return token.Type == JTokenType.Null ? default : token.Value<T>();
		}
	}
}
