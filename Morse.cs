namespace MornyMorse;

public class Morse
{
    public static readonly List<string> Bigrams = ["AB", "AC", "AD", "AE", "AF", "AG", "AL", "AM", "AN", "AR", "AS", "AT", "AU", "AV", "AW", "AY", "BA", "BE", "BI", "BL", "BO", "BR", "BU", "CA", "CE", "CH", "CI", "CL", "CO", "CR", "CU", "DA", "DE", "DI", "DO", "DR", "DU", "EA", "ED", "EE", "EF", "EI", "EL", "EM", "EN", "ER", "ES", "ET", "EV", "EW", "EX", "FA", "FE", "FI", "FL", "FO", "FR", "FU", "GA", "GE", "GI", "GL", "GO", "GR", "GU", "HA", "HE", "HI", "HO", "HU", "HY", "IA", "IE", "IG", "IL", "IM", "IN", "IO", "IR", "IS", "IT", "IV", "JA", "JE", "JO", "JU", "KA", "KE", "KI", "KN", "LA", "LE", "LI", "LO", "LU", "MA", "ME", "MI", "MO", "MU", "NA", "NE", "NI", "NO", "NU", "NY", "OA", "OB", "OC", "OD", "OE", "OF", "OG", "OI", "OL", "OM", "ON", "OO", "OP", "OR", "OS", "OT", "OU", "OW", "OX", "PA", "PE", "PI", "PL", "PO", "PR", "PU", "QU", "RA", "RE", "RI", "RO", "RU", "SA", "SC", "SE", "SH", "SI", "SL", "SO", "SP", "ST", "SU", "TA", "TE", "TH", "TI", "TO", "TR", "TU", "TW", "UA", "UE", "UL", "UM", "UN", "UP", "UR", "US", "UT", "VA", "VE", "VI", "VO", "WA", "WE", "WH", "WI", "WO", "WR", "XA", "XE", "XI", "YA", "YE", "YO", "ZA", "ZE"];
    public static readonly List<string> Trigrams = ["THE", "AND", "THA", "ENT", "ING", "ION", "TIO", "FOR", "NDE", "HAS", "NCE", "EDT", "TIS", "OFT", "STH", "MEN", "ONE"];
    public static readonly List<string> ProSigns = [
    "|AC",
    "|AR",
    "|BK",
    "|KW",
    "|KN",
    "|KK",
    "|DN",
    "|AS",
    "|CT",
    "|RN",
    "|HH",
    "|SA",
    "|SK",
    "|SX",
    "|VE"];

    public static readonly List<string> Punctuation = [
        ",",
        "?",
        ".",
        ",",
        "/",
        "="
        ];

    public static readonly List<string> Letters = [
    "A",
    "B",
    "C",
    "D",
    "E",
    "F",
    "G",
    "H",
    "I",
    "J",
    "K",
    "L",
    "M",
    "N",
    "O",
    "P",
    "Q",
    "R",
    "S",
    "T",
    "U",
    "V",
    "W",
    "X",
    "Y",
    "Z" ];

    public static readonly List<string> Words = ["THAT", "WITH", "HAVE", "THIS", "FROM", "THEY", "WILL", "WOULD", "THERE", "THEIR", "WHICH", "ABOUT", "THESE", "OTHER", "WORDS", "COULD", "WRITE", "FIRST", "THOSE", "BEING", "AFTER", "SHALL", "BELOW", "THING", "WHERE", "UNDER", "AGAIN", "EVERY", "NEVER", "MIGHT", "FOUND", "RIGHT", "GOING", "GREAT", "PLACE", "SMALL", "LARGE", "PEOPLE", "POINT", "WORLD", "WHILE", "SOUND", "AGAIN", "HOUSE", "GROUP", "TABLE", "STORY", "WHITE", "EARTH", "SENSE", "YOUNG", "THINK", "SHOULD", "LIGHT", "LATER", "PLACE", "LOOKS", "WATER", "THESE", "THINK", "WORDS", "CHILD", "SOUTH", "POWER", "SHORT", "REACH", "WHOLE", "ALONG", "TIMES", "STATE", "BLACK", "LEVEL", "LIVED", "RIVER", "ALONE", "FIELD", "DOING", "START", "WHOM", "BEGAN", "WHOSE", "ENTER", "ROUND", "HEARD", "LEARN", "BELOW", "FINAL", "PARTS", "ALWAYS", "UNDER", "HEART", "READY", "CLEAR", "TRUTH", "VOICE", "BRING", "POINT", "GIVEN"];


    public static readonly List<string> Numbers =
    [
        "0",
        "1",
        "2",
        "3",
        "4",
        "5",
        "6",
        "7",
        "8",
        "9"
    ];

    public static readonly Dictionary<string, string> ToMorse = new Dictionary<string, string>()
    {
    {"A", ".-"},
    {"B", "-..."},
    {"C", "-.-."},
    {"D", "-.."},
    {"E", "."},
    {"F", "..-."},
    {"G", "--."},
    {"H", "...."},
    {"I", ".."},
    {"J", ".---"},
    {"K", "-.-"},
    {"L", ".-.."},
    {"M", "--"},
    {"N", "-."},
    {"O", "---"},
    {"P", ".--."},
    {"Q", "--.-"},
    {"R", ".-."},
    {"S", "..."},
    {"T", "-"},
    {"U", "..-"},
    {"V", "...-"},
    {"W", ".--"},
    {"X", "-..-"},
    {"Y", "-.--"},
    {"Z", "--.."},
    {"0", "-----"},
    {"1", ".----"},
    {"2", "..---"},
    {"3", "...--"},
    {"4", "....-"},
    {"5", "....."},
    {"6", "-...."},
    {"7", "--..."},
    {"8", "---.."},
    {"9", "----."},
    {".", ".-.-.-"},
    {",", "--..--"},
    {":", "---..."},
    {";", "-.-.-."},
    {"+", ".-.-."},
    {"-", "-....-"},
    {"_", "..--.-"},
    {"\"", ".-..-."},
    //{"\"", ".----."},
    {" ", "/"},

    {"|BK", "-...-"},
    {"=", "-...-"},

    {"?", "..--.."},
    {"|SA", "..--.."},

    {"@", ".--.-."},
    {"|AC", ".--.-."},

    {"!", "-.-.--"},
    {"|KW", "-.-.--" },

    {"(", "-.--." },
    {"|KN", "-.--." }, // Invite to transmit

    {")", "-.--.-" },
    {"|KK", "-.--.-" },

    {"/", "-..-."}, // Slash
    {"|DN", "-..-." }, //
    
    {"&", ".-..."},
    {"|AS", ".-..." }, // Wait

    {"|CT", "-.-.-" }, // Commence transmission
    
    {"|RN", ".-.-." }, // New message follows

    {"|HH", "........" }, // Error
    
    {"|SK", "...-.-" }, // End of Work
    
    {"$", "...-..-"},
    {"|SX", "...-..-"},

    {"|AR", ".-.-." }, // Out

    {"|VE", "..._." } // Verified
    };

};

