// Library Checkout Terminal
// CheckedOut - raise event(name, bookTitle)

AuditedTerminal terminal = new AuditedTerminal();

// 1. SUBSCRIBE: This routes directly to your 'add' block
terminal.BookCheckedOut += OnBookBorrowed;

// 2. EXECUTE: Fires the event normally
terminal.CompleteCheckout("Alice", "C# Advanced Guide");

// 3. UNSUBSCRIBE: This routes directly to your 'remove' block!
terminal.BookCheckedOut -= OnBookBorrowed;

// 4. TEST: This will stay silent because the handler is detached
terminal.CompleteCheckout("Bob", "Clean Architecture");

// This named method matches the EventHandler<CheckoutEventArgs> signature
static void OnBookBorrowed(object? sender, CheckoutEventArgs e)
{
    Console.WriteLine($"[Notification sent] Book '{e.BookTitle}' checked out.");
}
//Console.WriteLine("--- Initializing Library Terminal --- \n");

//// 1. Instantiate the event publisher
//LibraryTerminal terminal = new();

//// 2. Subscribe to the event using a lambda expression
//terminal.BookCheckedOut += (sender, e) =>
//{
//    Console.WriteLine($"[AUDIT LOG]: User '{e.MemberName}' successfully borrowed '{e.BookTitle}'.");
//};

//// 3. Trigger the business method to fire the event pipeline
//terminal.CompleteCheckout("Alice Smith", "Design Patterns");
//terminal.CompleteCheckout("Bob Jones", "Clean Code");

//Console.WriteLine("\n--- Processing Complete ---");
//Console.ReadLine();

class CheckoutEventArgs : EventArgs
{
    public string? MemberName { get; set; }
    public string? BookTitle { get; set; }
}

class LibraryTerminal
{
    public event EventHandler<CheckoutEventArgs>? BookCheckedOut;

    public void CompleteCheckout(string member, string title)
    {
        BookCheckedOut?.Invoke(this, new CheckoutEventArgs
        {
            MemberName = member,
            BookTitle = title
        });
    }
}

class AuditedTerminal
{
    private EventHandler<CheckoutEventArgs>? _checkedOutHandlers;

    public event EventHandler<CheckoutEventArgs> BookCheckedOut
    {
        add
        {
            Console.WriteLine("A new listener has connected.");
            _checkedOutHandlers += value;
        }
        remove
        {
            Console.WriteLine("A listener has disconnected.");
            _checkedOutHandlers -= value;
        }
    }

    public void CompleteCheckout(string member, string title)
    {
        _checkedOutHandlers?.Invoke(this, new CheckoutEventArgs
        {
            MemberName = member,
            BookTitle = title
        });
    }
}