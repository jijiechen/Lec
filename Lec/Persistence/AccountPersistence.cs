using System.Threading.Tasks;
using ACMESharp.Protocol;

namespace Lec.Persistence
{
    class AccountPersistence
    {
        public static async Task<AccountDetails> CreateNew(AcmeProtocolClient client, string contactEmail)
        {
            var contacts = new[] { "mailto:" + contactEmail };
            return await client.CreateAccountAsync(contacts, true);
        }

        public static AccountDetails LoadFromFile(string filePath)
        {
            var account = new AccountDetails();
            FileUtils.LoadStateInto(ref account, filePath, true);
            return account;
        }

        public static void SaveToFile(AccountDetails account, string filePath)
        {
            FileUtils.SaveStateFrom(account, filePath);
        }
    }
}
