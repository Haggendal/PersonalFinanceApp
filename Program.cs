namespace PersonalFinanceApp
{
    class Program
    {
        private static string connectionString = "Host=localhost;Username=postgres;Password=password;Database=personalfinance";
        private static TransactionManager? transactionManager;
        private static string? currentUsername;
        private static int currentUserId;

        static void Main(string[] args)
        {
            try
            {
                DatabaseInitializer.Initialize(connectionString);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to connect to database.");
                Console.WriteLine($"Error: {ex.Message}");
                return;
            }

            while (true)
            {
                if (currentUserId == 0)
                {
                    DisplayLoginMenu();
                }
                else
                {
                    DisplayMenu();
                }
            }
        }

        static void DisplayLoginMenu()
        {
            Console.Clear();
            Console.WriteLine($"{TextStyling.CYAN}====================================={TextStyling.NORMAL}");
            Console.WriteLine($"{TextStyling.CYAN}||  {TextStyling.BOLD}{TextStyling.MAGENTA}Personal Finance App - Login{TextStyling.NOBOLD}{TextStyling.CYAN}   ||{TextStyling.NORMAL}");
            Console.WriteLine($"{TextStyling.CYAN}====================================={TextStyling.NORMAL}");
            Console.WriteLine($"{TextStyling.CYAN}||  [{TextStyling.ORANGE}1{TextStyling.CYAN}]{TextStyling.YELLOW} Login{TextStyling.CYAN}                      ||");
            Console.WriteLine($"{TextStyling.CYAN}||  [{TextStyling.ORANGE}2{TextStyling.CYAN}]{TextStyling.YELLOW} Register{TextStyling.CYAN}                   ||");
            Console.WriteLine($"{TextStyling.CYAN}||  [{TextStyling.ORANGE}3{TextStyling.CYAN}]{TextStyling.YELLOW} Exit{TextStyling.CYAN}                       ||");
            Console.WriteLine($"{TextStyling.CYAN}====================================={TextStyling.NORMAL}");
            Console.Write($"{TextStyling.NOBOLD}Select an option: ");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    HandleLogin();
                    break;
                case "2":
                    HandleRegistration();
                    break;
                case "3":
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Invalid option. Press any key to continue...");
                    Console.ReadKey();
                    break;
            }
        }

        static void HandleLogin()
        {
            Console.Write("\nUsername: ");
            string username = Console.ReadLine();
            Console.Write("Password: ");
            string password = Console.ReadLine();

            if (User.Login(connectionString, username, password))
            {
                currentUsername = username;
                currentUserId = User.GetUserId(connectionString, username);
                transactionManager = new TransactionManager(connectionString, currentUserId);
                Console.WriteLine("\nLogin successful! Press any key to continue...");
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine("\nPress any key to try again...");
                Console.ReadKey();
            }
        }

        static void HandleRegistration()
        {
            Console.Write("\nUsername: ");
            string username = Console.ReadLine();
            Console.Write("Password: ");
            string password = Console.ReadLine();

            User.CreateAccount(connectionString, username, password);
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        static void DisplayMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"{TextStyling.CYAN}====================================={TextStyling.NORMAL}");
                Console.WriteLine($"{TextStyling.CYAN}||    {TextStyling.BOLD}{TextStyling.MAGENTA}Personal Finance App{TextStyling.NOBOLD}{TextStyling.CYAN}         ||{TextStyling.NORMAL}");
                Console.WriteLine($"{TextStyling.CYAN}====================================={TextStyling.NORMAL}");
                Console.WriteLine($"{TextStyling.CYAN}||  [{TextStyling.ORANGE}1{TextStyling.CYAN}]{TextStyling.YELLOW} Add Transaction{TextStyling.CYAN}            ||");
                Console.WriteLine($"{TextStyling.CYAN}||  [{TextStyling.ORANGE}2{TextStyling.CYAN}]{TextStyling.YELLOW} Delete Transaction{TextStyling.CYAN}         ||");
                Console.WriteLine($"{TextStyling.CYAN}||  [{TextStyling.ORANGE}3{TextStyling.CYAN}]{TextStyling.YELLOW} Current Balance{TextStyling.CYAN}            ||");
                Console.WriteLine($"{TextStyling.CYAN}||  [{TextStyling.ORANGE}4{TextStyling.CYAN}]{TextStyling.YELLOW} Search Income{TextStyling.CYAN}              ||");
                Console.WriteLine($"{TextStyling.CYAN}||  [{TextStyling.ORANGE}5{TextStyling.CYAN}]{TextStyling.YELLOW} Search Expenses{TextStyling.CYAN}            ||");
                Console.WriteLine($"{TextStyling.CYAN}||  [{TextStyling.ORANGE}6{TextStyling.CYAN}]{TextStyling.YELLOW} Logout{TextStyling.CYAN}                     ||");
                Console.WriteLine($"{TextStyling.CYAN}||  [{TextStyling.ORANGE}7{TextStyling.CYAN}]{TextStyling.YELLOW} Exit{TextStyling.CYAN}                       ||");
                Console.WriteLine($"{TextStyling.CYAN}====================================={TextStyling.NORMAL}");
                Console.WriteLine($"{TextStyling.GREY}Logged in as: {TextStyling.YELLOW}{currentUsername}{TextStyling.NORMAL}");
                Console.WriteLine($"{TextStyling.CYAN}====================================={TextStyling.NORMAL}");
                Console.Write($"{TextStyling.NOBOLD}Select an option: ");

                var choice = Console.ReadLine();
                Console.WriteLine();

                switch (choice)
                {
                    case "1":
                        AddTransaction();
                        break;
                    case "2":
                        DeleteTransaction();
                        break;
                    case "3":
                        ShowBalance();
                        break;
                    case "4":
                        SearchTransactions("Income");
                        break;
                    case "5":
                        SearchTransactions("Expense");
                        break;
                    case "6":
                        Logout();
                        return;
                    case "7":
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please select 1-7.");
                        break;
                }

                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
            }
        }

        static void AddTransaction()
        {
            try
            {
                Console.Write($"Enter Date ({TextStyling.ORANGE}yyyy{TextStyling.NORMAL}-{TextStyling.ORANGE}mm{TextStyling.NORMAL}-{TextStyling.ORANGE}dd{TextStyling.NORMAL}) or '{TextStyling.ORANGE}X{TextStyling.NORMAL}' to exit: ");
                string dateInput = Console.ReadLine();
                if (dateInput?.ToUpper() == "X") return;

                if (!DateTime.TryParse(dateInput, out DateTime date))
                {
                    Console.WriteLine("Invalid date format.");
                    return;
                }

                Console.Write($"Enter Amount or '{TextStyling.ORANGE}X{TextStyling.NORMAL}' to exit: ");
                string amountInput = Console.ReadLine();
                if (amountInput?.ToUpper() == "X") return;

                if (!decimal.TryParse(amountInput, out decimal amount))
                {
                    Console.WriteLine("Invalid amount.");
                    return;
                }

                string type;
                do
                {
                    Console.Write($"Enter Type ({TextStyling.GREEN}Income{TextStyling.NORMAL}/{TextStyling.RED}Expense{TextStyling.NORMAL}) or '{TextStyling.ORANGE}X{TextStyling.NORMAL}' to exit: ");
                    type = Console.ReadLine()?.Trim() ?? "";
                    if (type.ToUpper() == "X") return;
                } while (type.ToLower() != "income" && type.ToLower() != "expense");

                Console.Write($"Enter Description or '{TextStyling.ORANGE}X{TextStyling.NORMAL}' to exit: ");
                string description = Console.ReadLine();
                if (description?.ToUpper() == "X") return;

                transactionManager.AddTransaction(date, amount, type, description ?? "");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static void DeleteTransaction()
        {
            try
            {
                var transactions = transactionManager.SearchTransactions();
                int i = 0;

                foreach (var t in transactions)
                {
                    string color = t.Type == "Income" ? TextStyling.GREEN : TextStyling.RED;
                    string amountDisplay = TextStyling.ApplyColor(t.Amount.ToString("N2"), color);

                    Console.WriteLine($"[{TextStyling.ORANGE}{i}{TextStyling.NORMAL}] Date: {t.Date.ToShortDateString()}, {t.Type}, Amount: {amountDisplay}, Description: {t.Description}");
                    i++;
                }

                Console.Write($"\nSelect transaction to delete (0-{i - 1}) or '{TextStyling.ORANGE}X{TextStyling.NORMAL}' to exit: ");
                var input = Console.ReadLine();

                if (input?.ToUpper() == "X") return;

                if (int.TryParse(input, out int index) && transactionManager.DeleteTransaction(index))
                {
                    Console.WriteLine("Transaction deleted successfully.");
                }
                else
                {
                    Console.WriteLine("Invalid selection.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static void ShowBalance()
        {
            decimal balance = transactionManager.GetCurrentBalance();
            string color = balance > 0 ? TextStyling.GREEN : (balance < 0 ? TextStyling.RED : TextStyling.ORANGE);
            string balanceDisplay = TextStyling.ApplyColor(balance.ToString("N2"), color);
            Console.WriteLine($"{TextStyling.BOLD}{TextStyling.GREY}Current Balance: {balanceDisplay}{TextStyling.NOBOLD}{TextStyling.NORMAL}");
        }

        static void SearchTransactions(string type)
        {
            Console.Clear();
            Console.WriteLine($"{TextStyling.CYAN}Search {type} by:{TextStyling.NORMAL}");
            Console.WriteLine($"[{TextStyling.ORANGE}1{TextStyling.NORMAL}] Year");
            Console.WriteLine($"[{TextStyling.ORANGE}2{TextStyling.NORMAL}] Month");
            Console.WriteLine($"[{TextStyling.ORANGE}3{TextStyling.NORMAL}] Week");
            Console.WriteLine($"[{TextStyling.ORANGE}4{TextStyling.NORMAL}] Day");
            Console.WriteLine($"[{TextStyling.ORANGE}X{TextStyling.NORMAL}] Exit Menu");
            Console.Write("Select an option: ");
            var choice = Console.ReadLine();

            if (choice?.ToUpper() == "X") return;

            int? year = null, month = null, week = null;
            DateTime? date = null;
            string filterType = "";

            try
            {
                switch (choice)
                {
                    case "1":
                        Console.Write($"\nEnter Year ({TextStyling.ORANGE}yyyy{TextStyling.NORMAL}): ");
                        if (int.TryParse(Console.ReadLine(), out int y))
                        {
                            year = y;
                            filterType = "Year";
                        }
                        break;
                    case "2":
                        Console.Write($"\nEnter Month ({TextStyling.ORANGE}1-12{TextStyling.NORMAL}): ");
                        if (int.TryParse(Console.ReadLine(), out int m) && m >= 1 && m <= 12)
                        {
                            month = m;
                            Console.Write($"Enter Year ({TextStyling.ORANGE}yyyy{TextStyling.NORMAL}): ");
                            if (int.TryParse(Console.ReadLine(), out y))
                            {
                                year = y;
                                filterType = "Month";
                            }
                        }
                        break;
                    case "3":
                        Console.Write($"\nEnter Week ({TextStyling.ORANGE}1-53{TextStyling.NORMAL}): ");
                        if (int.TryParse(Console.ReadLine(), out int w) && w >= 1 && w <= 53)
                        {
                            week = w;
                            Console.Write($"Enter Year ({TextStyling.ORANGE}yyyy{TextStyling.NORMAL}): ");
                            if (int.TryParse(Console.ReadLine(), out y))
                            {
                                year = y;
                                filterType = "Week";
                            }
                        }
                        break;
                    case "4":
                        Console.Write($"\nEnter Date ({TextStyling.ORANGE}yyyy-mm-dd{TextStyling.NORMAL}): ");
                        if (DateTime.TryParse(Console.ReadLine(), out DateTime d))
                        {
                            date = d;
                            filterType = "Day";
                        }
                        break;
                }

                var results = transactionManager.SearchTransactions(type, filterType, year, month, week, date);
                decimal total = results.Sum(t => t.Amount);

                string totalColor;
                if (type == "Income")
                {
                    totalColor = total > 0 ? TextStyling.GREEN : TextStyling.RED;
                }
                else
                {
                    totalColor = total > 0 ? TextStyling.RED : TextStyling.GREEN;
                }

                Console.WriteLine($"\n{TextStyling.BOLD}Total {type}: {TextStyling.ApplyColor(total.ToString("N2"), totalColor)}{TextStyling.NOBOLD}");
                Console.WriteLine($"\n{TextStyling.CYAN}Transactions:{TextStyling.NORMAL}");

                foreach (var t in results)
                {
                    string transactionColor = t.Type == "Income" ? TextStyling.GREEN : TextStyling.RED;
                    Console.WriteLine($"Date: {t.Date.ToShortDateString()}, Amount: {TextStyling.ApplyColor(t.Amount.ToString("N2"), transactionColor)}, Description: {t.Description}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static void Logout()
        {
            currentUsername = null;
            currentUserId = 0;
            transactionManager = null;
            Console.WriteLine("Logged out successfully.");
        }
    }
}