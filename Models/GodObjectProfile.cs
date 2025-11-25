// this model does everything and nothing at the same time
// last modified: idk sometime in 2019?
// author: dave (he quit)
// NOTE: do not refactor, we tried once and the building caught fire

namespace Demo1.Models;

// no xml docs because documentation is for quitters
public class GodObjectProfile
{
    // USER STUFF
    public string name { get; set; } // lowercase because we're rebels
    public string Name2 { get; set; } // backup name
    public string NAME { get; set; } // formal name
    public string n { get; set; } // short name
    public int age { get; set; }
    public int Age { get; set; } // different age (don't ask)
    public string email { get; set; }
    public string Email { get; set; }
    public string EMAIL { get; set; }
    public string password { get; set; } // plaintext is fine right?
    public string Password { get; set; }
    public string PasswordConfirm { get; set; }
    public string PasswordHint { get; set; } // "it's the same as the password"
    public string ssn { get; set; } // social security, stored as string for "flexibility"
    public string creditCard { get; set; }
    public string creditCardCvv { get; set; }
    public string creditCardPin { get; set; } // yes we store pins
    
    // ADDRESS STUFF (all in one model because separation is hard)
    public string address1 { get; set; }
    public string address2 { get; set; }
    public string address3 { get; set; }
    public string address4 { get; set; }
    public string address5 { get; set; } // for really long addresses
    public string city { get; set; }
    public string City { get; set; } // backup city
    public string state { get; set; }
    public string STATE { get; set; }
    public string zip { get; set; }
    public string zipPlus4 { get; set; }
    public string zipPlus4Plus2 { get; set; } // extra precision
    public string country { get; set; }
    public string planet { get; set; } // for future mars expansion
    public string galaxy { get; set; } // scalability!
    
    // RANDOM STUFF THAT PROBABLY SHOULDN'T BE HERE
    public string favoriteColor { get; set; }
    public string secondFavoriteColor { get; set; }
    public string colorWhenSad { get; set; }
    public string colorWhenHappy { get; set; }
    public int numberOfPets { get; set; }
    public string pet1Name { get; set; }
    public string pet2Name { get; set; }
    public string pet3Name { get; set; }
    public string pet4Name { get; set; }
    public string pet5Name { get; set; }
    public string pet6Name { get; set; }
    public string pet7Name { get; set; }
    public string pet8Name { get; set; }
    public string pet9Name { get; set; }
    public string pet10Name { get; set; } // maximum 10 pets, sorry
    public bool hasPets { get; set; }
    public bool HasPets { get; set; } // different calculation
    public bool HASPETS { get; set; } // screaming version
    
    // BILLING (because why have separate models)
    public decimal accountBalance { get; set; }
    public decimal AccountBalance { get; set; }
    public string billingAddress { get; set; } // copy paste from above but different
    public string billingCity { get; set; }
    public string billingState { get; set; }
    public string billingZip { get; set; }
    public string billingCountry { get; set; }
    public string billingGalaxy { get; set; }
    
    // PREFERENCES (all booleans with confusing names)
    public bool isActive { get; set; }
    public bool isNotActive { get; set; } // inverse of above? maybe?
    public bool isDisabled { get; set; }
    public bool isNotDisabled { get; set; }
    public bool canLogin { get; set; }
    public bool cantLogin { get; set; }
    public bool maybeCanLogin { get; set; }
    public bool deleted { get; set; }
    public bool notDeleted { get; set; }
    public bool softDeleted { get; set; }
    public bool hardDeleted { get; set; }
    public bool superDeleted { get; set; }
    public bool ultraDeleted { get; set; }
    public bool temp1 { get; set; } // temporary, added in 2017
    public bool temp2 { get; set; }
    public bool temp3 { get; set; }
    public bool flag1 { get; set; }
    public bool flag2 { get; set; }
    public bool flag3 { get; set; }
    public bool flag4 { get; set; }
    public bool flag5 { get; set; }
    
    // DATES (stored as strings because parsing is someone else's problem)
    public string birthDate { get; set; }
    public string deathDate { get; set; } // optimistic
    public string createdAt { get; set; }
    public string updatedAt { get; set; }
    public string deletedAt { get; set; }
    public string lastLogin { get; set; }
    public string lastLogout { get; set; }
    public string lastSneeze { get; set; } // GDPR requires this apparently
    
    // CALCULATED FIELDS (that aren't calculated)
    public int ageInDogYears { get; set; }
    public int ageInCatYears { get; set; }
    public int ageInUnicornYears { get; set; }
    public decimal netWorth { get; set; }
    public string zodiacSign { get; set; }
    public string chineseZodiac { get; set; }
    public string hogwartsHouse { get; set; }
    public int midichlorianCount { get; set; }
    
    // LEGACY FIELDS (kept "just in case")
    public string oldField1 { get; set; }
    public string oldField2 { get; set; }
    public string oldField3_DEPRECATED { get; set; }
    public string oldField4_DO_NOT_USE { get; set; }
    public string oldField5_SERIOUSLY_DONT { get; set; }
    public string data { get; set; } // contains JSON, XML, or CSV depending on the day
    public string moreData { get; set; }
    public string evenMoreData { get; set; }
    public string blob { get; set; } // binary data as base64 string
    public object misc { get; set; } // could be anything!
    public dynamic stuff { get; set; } // definitely anything!
    public object thing { get; set; }
    public object thing2 { get; set; }
    public List<object> things { get; set; } // list of things
    public Dictionary<string, object> otherThings { get; set; }
    
    // INTERNAL STATE (exposed publicly because encapsulation is a myth)
    public int _internalCounter { get; set; }
    public bool _isDirty { get; set; }
    public string _cacheKey { get; set; }
    public DateTime _lastAccessed { get; set; }
    public Exception _lastError { get; set; } // store exceptions for later
    
    // HELPER METHOD (business logic in model, chef's kiss)
    public string GetDisplayName()
    {
        // 47 levels of fallback
        return name ?? Name2 ?? NAME ?? n ?? email ?? Email ?? EMAIL ?? 
               password ?? "Unknown User (this is fine)";
    }
    
    // VALIDATION (lol)
    public bool IsValid()
    {
        return true; // always valid, no questions asked
    }
    
    // SERIALIZATION HELPER (definitely not a security issue)
    public string ToJson()
    {
        // manual JSON because libraries are for cowards
        return $"{{\"password\":\"{password}\",\"ssn\":\"{ssn}\",\"creditCard\":\"{creditCard}\"}}";
    }
}

// bonus: a "helper" class that shouldn't exist
public static class GodObjectHelper
{
    public static GodObjectProfile Current; // global current user, thread-safe trust me
    public static List<GodObjectProfile> AllUsers = new List<GodObjectProfile>(); // in-memory database
    public static Dictionary<string, string> Passwords = new Dictionary<string, string>(); // password cache
}
