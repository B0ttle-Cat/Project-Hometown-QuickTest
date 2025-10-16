public static class KoreanHelper
{
	// 받침 여부 확인
	public static bool HasFinalConsonant(string word)
	{
		if (string.IsNullOrEmpty(word))
			return false;

		char lastChar = word[word.Length - 1];

		// 한글 음절 범위: '가'(0xAC00) ~ '힣'(0xD7A3)
		if (lastChar < 0xAC00 || lastChar > 0xD7A3)
			return false; // 한글이 아니면 받침 없다고 처리

		int unicodeIndex = lastChar - 0xAC00;
		int jongseongIndex = unicodeIndex % 28;

		return jongseongIndex != 0;
	}

	// 조사 붙이기
	/// <summary>
	/// <cdoe>받침 "있음|없음"
	/// <br> ex) 은/는, 이/가, 을/를, 과/와, 아/야, 이여/여, 이랑/랑, 으로/로, 으로서/로서, 으로써/로써, 으로부터/로부터 </br></cdoe>
	/// </summary>
	public static string Josa(string word, string josaPair)
	{
		if (string.IsNullOrEmpty(word) || string.IsNullOrEmpty(josaPair) || josaPair.Length != 3)
			return word;

		string[] parts = josaPair.Split('/');
		if (parts.Length != 2)
			return word;

		string josa = HasFinalConsonant(word) ? parts[0] : parts[1];
		return word + josa;
	}
	/// <summary>
	/// <cdoe>받침 "있음|없음"
	/// <br> ex) 은/는, 이/가, 을/를, 과/와, 아/야, 이여/여, 이랑/랑, 으로/로, 으로서/로서, 으로써/로써, 으로부터/로부터 </br></cdoe>
	/// </summary>
	public static string Josa(int number, string josaPair)
	{
		if (string.IsNullOrEmpty(josaPair) || josaPair.Length != 3)
			return number.ToString();

		string[] parts = josaPair.Split('/');
		if (parts.Length != 2) return "";

		number = number % 10;
		bool 받침 = number switch
        {
            0 => true, //	영 / 십
			1 => true, //	일
			2 => false, //	이
			3 => true, //	삼
			4 => false, //	사
			5 => false, //	오
			6 => true, //	육
			7 => true, //	칠
			8 => true, //	팔
			9 => false, //	구
            _ => false,
        };


		string josa = 받침 ? parts[0] : parts[1];
		return $"{number}{josa}";
	}
	/// <summary>
	/// <cdoe>받침 "있음|없음"
	/// <br> ex) 은/는, 이/가, 을/를, 과/와, 아/야, 이여/여, 이랑/랑, 으로/로, 으로서/로서, 으로써/로써, 으로부터/로부터 </br></cdoe>
	/// </summary>
	public static string OnlyJosa(string word, string josaPair)
	{
		if (string.IsNullOrEmpty(word) || string.IsNullOrEmpty(josaPair) || josaPair.Length != 3)
			return josaPair;

		string[] parts = josaPair.Split('/');
		if (parts.Length != 2)
			return word;

		return HasFinalConsonant(word) ? parts[0] : parts[1];
	}
	/// <summary>
	/// <cdoe>받침 "있음|없음"
	/// <br> ex) 은/는, 이/가, 을/를, 과/와, 아/야, 이여/여, 이랑/랑, 으로/로, 으로서/로서, 으로써/로써, 으로부터/로부터 </br></cdoe>
	/// </summary>
	public static string OnlyJosa(int number, string josaPair)
	{
		if (string.IsNullOrEmpty(josaPair) || josaPair.Length != 3)
			return josaPair;

		string[] parts = josaPair.Split('/');
		if (parts.Length != 2) return "";

		number = number % 10;
		bool 받침 = number switch
		{
			0 => true, //	영 / 십
			1 => true, //	일
			2 => false, //	이
			3 => true, //	삼
			4 => false, //	사
			5 => false, //	오
			6 => true, //	육
			7 => true, //	칠
			8 => true, //	팔
			9 => false, //	구
            _ => false,
		};
		return 받침 ? parts[0] : parts[1];
	}
}
