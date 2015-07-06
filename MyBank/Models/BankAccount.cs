namespace MyBank.Models
{
    /// <summary>
    /// Klasse für ein Konto
    /// </summary>
    public class BankAccount
    {
        /// <summary>
        /// Die interne Datenbank-ID
        /// </summary>
        public int BankAccountId { get; set; }

        /// <summary>
        /// Die öffentliche Kontonummer
        /// </summary>
        public int Number { get; set; }

        private double _balance = 0;

        /// <summary>
        /// Das Guthaben auf dem Konto
        /// </summary>
        public double Balance { get { return _balance; } internal set { _balance = value; } }

        /// <summary>
        /// Die ID des Benutzers, dem das Konto gehört
        /// </summary>
        public string OwnerId { get; set; }

        /// <summary>
        /// Das Objekt des Besitzers, wird vom EF über die ID automatisch zugeordnet
        /// </summary>
        public virtual ApplicationUser Owner { get; set; }

        /// <summary>
        /// Ein leerer Konstruktor für die Erstellung aus der Datenbank / Deserialisierung
        /// </summary>
        public BankAccount()
        {
        }

        /// <summary>
        /// Standardkonstruktor
        /// </summary>
        /// <param name="ownerId"></param>
        /// <param name="number"></param>
        public BankAccount(string ownerId, int number)
        {
            OwnerId = ownerId;
            Balance = 0;
            Number = number;
        }

        /// <summary>
        /// Methode um Geld abzuheben
        /// </summary>
        /// <param name="amount">Die Summe der Abhebung</param>
        /// <returns>True bei Erfolg, False falls die Deckung nicht ausreicht</returns>
        public bool Withdraw(double amount)
        {
            if (amount > Balance)
                return false;

            Balance -= amount;
            return true;
        }

        /// <summary>
        /// Methode zum Einzahlen
        /// </summary>
        /// <param name="amount"></param>
        public void PayIn(double amount)
        {
            Balance += amount;
        }

        /// <summary>
        /// Methode zur Überweisung von Geld auf ein anderes Konto
        /// </summary>
        /// <param name="amount">Die Summer der Überweisung</param>
        /// <param name="otherAccount">Die (öffentliche) Kontonummer des anderen Accounts</param>
        /// <returns>True bei Erfolg, False falls die Deckung nicht ausreicht</returns>
        public bool Transfer(double amount, BankAccount otherAccount)
        {
            if (amount > Balance)
                return false;

            Balance -= amount;
            otherAccount.PayIn(amount);
            return true;
        }
    }
}