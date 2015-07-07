In diesem Repository befinden sich zwei Projekte, die eine Einführung in .NET-Technologien bieten:
- "MyBank" ist eine Web-App mit ASP.NET MVC 5 mit Entity Framework 6
- "MyBankAdmin" ist ein WPF-Programm, das via Entity Framework 6 auf dieselbe Datenbank zugreift.
Die Anleitungen sind hier oder im Ordner "Turorial" zu finden.

## Inhalt
1. [Beispielprojekt "MyBank"](#beispielprojekt-mybank)
  1. [Projekt erstellen](#projekt-erstellen)
  2. [Model-Klassen erstellen](#model-klassen-erstellen)
  3. [Einfügen der Create-Methode](#einfügen-der-create-methode)
  4. [Weitere Methoden und Views](#weitere-methoden-und-views)
  5. [Erweiterung zum ViewModel](#erweiterung-zum-viewmodel)
2. [Beispielprojekt "MyBankAdmin"](#beispielprojekt-mybankadmin)
  1. [Datenbankanbindung einfügen](#datenbankanbindung-einfügen)
  2. [Einträge anzeigen](#einträge-anzeigen)
  3. [Bindings zwischen Model und View](#bindings-zwischen-model-und-view)
  4. [Einträge bearbeiten](#einträge-bearbeiten)

# Beispielprojekt "MyBank"
ASP.NET MVC 5 mit Entity Framework 6

## Projekt erstellen

Visual Studio 2015 öffnen:

![](https://raw.githubusercontent.com/JannikArndt/MyBank/master/Tutorial/image1.png)

Neues Projekt erstellen:

![](https://raw.githubusercontent.com/JannikArndt/MyBank/master/Tutorial/image2.png)

ASP.NET Web Application auswählen und Namen eingeben:

![](https://raw.githubusercontent.com/JannikArndt/MyBank/master/Tutorial/image3.png)

MVC Template auswählen (Authentication aus Individual User Accounts):

![](https://raw.githubusercontent.com/JannikArndt/MyBank/master/Tutorial/image4.png)

Es ist ein leeres Projekt entstanden:

![](https://raw.githubusercontent.com/JannikArndt/MyBank/master/Tutorial/image5.png)

Mit einem Klick auf „Internet Explorer" kann das schon ausgeführt werden:

![](https://raw.githubusercontent.com/JannikArndt/MyBank/master/Tutorial/image6.png)

## Model-Klassen erstellen

Debuggen beenden (Shift + F5 oder rotes Quadrat) und eine neue Datei in den Ordner Models einfügen:

![](https://raw.githubusercontent.com/JannikArndt/MyBank/master/Tutorial/image7.png)

![](https://raw.githubusercontent.com/JannikArndt/MyBank/master/Tutorial/image8.png)

Inhalt der Klasse:

```csharp
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
```

Einfügen der Tabelle in den Datenbank-Kontext (hier in IdentityModels.cs):

![](https://raw.githubusercontent.com/JannikArndt/MyBank/master/Tutorial/image9.png)

Anpassung des HomeControllers (Back-End für den View):

1. 1.Klassenvariablen und Konstruktor:
```csharp
private ApplicationDbContext \_bank { get; set; }

private ApplicationUserManager \_userManager { get { return HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); } }

public HomeController()
{
    \_bank = new ApplicationDbContext();
}
```
1. 2.Index-Methode:
```csharp
[Authorize]
public ActionResult Index()
{
    var userId = \_userManager.FindById(User.Identity.GetUserId()).Id;
    var accounts = \_bank.Accounts.Where(account => account.OwnerId == userId).ToList();
    return View(accounts);
}
```

![](https://raw.githubusercontent.com/JannikArndt/MyBank/master/Tutorial/image10.png)

Anpassung der View (View > Home > Index.cshtml):
```html
@model List<MyBank.Models.BankAccount>

<h2>Meine Konten</h2>

<table class="table">
    <tr>
        <td>Kontonummer</td>
        <td>Guthaben</td>
        <td>Aktion</td>
    </tr>
    @foreach (var bankAccount in Model)
    {
        <tr>
            <td>@bankAccount.Number</td>
            <td>@bankAccount.Balance €</td>
            <td>
                @Html.ActionLink("Abheben", "Withdraw", new { account = bankAccount.Number }, new { @class = "btn btn-default" })
                @Html.ActionLink("Einzahlen", "PayIn", new { account = bankAccount.Number }, new { @class = "btn btn-default" })
                @Html.ActionLink("Überweisen", "Transfer", new { account = bankAccount.Number }, new { @class = "btn btn-default" })
            </td>
        </tr>
    }
</table>

@Html.ActionLink("Neues Konto", "Create", null, new { @class = "btn btn-default" })
```

![](https://raw.githubusercontent.com/JannikArndt/MyBank/master/Tutorial/image11.png)

Mit einem Klick auf „Internet Explorer" testen. Es kommt ein Log In-Dialog, da die Index-Methode das [Authorize]–Attribut hat:

![](https://raw.githubusercontent.com/JannikArndt/MyBank/master/Tutorial/image12.png)

Beim Registrieren eines Benutzers kommt eine Fehlermeldung:

![](https://raw.githubusercontent.com/JannikArndt/MyBank/master/Tutorial/image13.png)

Das Model hat sich geändert, die Datenbank wurde aber noch nicht aktualisiert.  Also Debugging beenden und Package Manager Console öffnen:

![](https://raw.githubusercontent.com/JannikArndt/MyBank/master/Tutorial/image14.png)

Mit dem Befehl

PM> Enable-Migrations -EnableAutomaticMigrations

die automatische Migration aktivieren. Im _SQL Server Object Explorer_ (links) kann man nun die frisch erstellte Datenbank anschauen. Die Verbindung hierzu ist i.d.R. (localdb)\MSSQLLocalDB, alternativ ist die genutzte Datenquelle auch in der Datei _Web.config_ (mitte) im Hauptverzeichnis eingetragen:

![](https://raw.githubusercontent.com/JannikArndt/MyBank/master/Tutorial/image15.png)

Der Aufruf von View Data zeigt aber, dass noch keine Daten vorhanden sind:

![](https://raw.githubusercontent.com/JannikArndt/MyBank/master/Tutorial/image16.png)

Nun also zurück zur Anwendung. Dafür das Debuggen erneut starten und einen Benutzer registrieren. Dieses mal klappt es. Es werden alle 0 Konten aufgelistet, die zum Benutzer gehören:

![](https://raw.githubusercontent.com/JannikArndt/MyBank/master/Tutorial/image17.png)

Der Klick auf _Neues Konto_ führt noch zu einer Fehlermeldung:

![](https://raw.githubusercontent.com/JannikArndt/MyBank/master/Tutorial/image18.png)

Es wird die URL _/Home/Create_ aufgerufen. Erwartet wird also im _HomeController.cs_ eine Methode mit dem Namen _Create()_.

## Einfügen der Create-Methode

Im HomeController.cs wird nun die Methode Create() eingefügt:
```csharp
[Authorize]
public RedirectToRouteResult Create()
{
    // Falls es schon Konten gibt, finde die höchste Kontonummer von allen und addiere 1, sonst nimm 1:
    var accountNumber = _bank.Accounts.Any() ? _bank.Accounts.Max(account => account.Number) + 1 : 1;

    // Erstelle ein neues Konto mit der aktuellen UserId als Besitzer und der eben herausgefundenen Kontonummer:
    var newAccount = new BankAccount(User.Identity.GetUserId(), accountNumber);

    // Füge das Konto der Liste der Konten der Bank hinzu:
    _bank.Accounts.Add(newAccount);

    // Speichere die Änderungen in die Datenbank:
    _bank.SaveChanges();

    // Zeige die Übersichtsseite (Index) an:
    return RedirectToAction("Index");
}
```

![](https://raw.githubusercontent.com/JannikArndt/MyBank/master/Tutorial/image19.png)

Mit einem Klick auf _Neues Konto_ wird nun ein neues Konto angelegt und der Liste hinzugefügt:

![](https://raw.githubusercontent.com/JannikArndt/MyBank/master/Tutorial/image20.png)

Diese Änderungen findet man auch gleich in der Datenbank wieder:

![](https://raw.githubusercontent.com/JannikArndt/MyBank/master/Tutorial/image21.png)

Leider führen die Methoden zum Einzahlen, Abheben und Überweisen noch nirgends hin.

## Weitere Methoden und Views

Die Buttons in der Liste verweisen bereits auf die Methoden PayIn, Withdraw und Transfer, die wir im HomeController erstellen müssen. Allerdings sind für diese Aktionen weitere Benutzereingaben nötig, das heißt wir benötigen einen eigenen View für jede Methode:

![](https://raw.githubusercontent.com/JannikArndt/MyBank/master/Tutorial/image22.png)

![](https://raw.githubusercontent.com/JannikArndt/MyBank/master/Tutorial/image23.png)

![](https://raw.githubusercontent.com/JannikArndt/MyBank/master/Tutorial/image24.png)

![](https://raw.githubusercontent.com/JannikArndt/MyBank/master/Tutorial/image25.png)

View.cshtml:
```html
@model int
    
@{
    Layout = "~/Views/Shared/_Layout.cshtml";
}

<h2>Einzahlen</h2>

@using (Html.BeginForm("PayIn", "Home", FormMethod.Post, new { @class = "form" }))
{
    @Html.TextBox("amount", "", new { @class = "form-control" })
    @Html.Hidden("account", Model.ToString())
    <br />
    <input type="submit" value="Einzahlen" class="btn" />
}
```

Für jede der Aktionen benötigen wir zwei Aufrufe zum Server: Beim ersten wird der View generiert, beim zweiten werden die eigegebenen Daten (z.B. der Betrag) übermittelt und die Logik durchgeführt. Hierfür kann man zwei verschiedene Methoden benutzen (z.B. PayIn und PayInDo), oder dieselbe Methode überladen und von der Art des Aufrufs (GET oder POST) abhängig machen, welche gewählt wird.

Ruft der Benutzer die URL auf, wird die GET-Methode ausgeführt, schickt er dann das Formular ab, wird dieses per POST übermittelt:

![](https://raw.githubusercontent.com/JannikArndt/MyBank/master/Tutorial/image26.png)

```csharp
[HttpGet]
[Authorize]
public ActionResult PayIn(int account)
{
    return View(account);
}

[HttpPost]
[Authorize]
public RedirectToRouteResult PayIn(int account, double amount)
{
    var myAccount = _bank.Accounts.FirstOrDefault(acc => acc.Number == account);
    if (myAccount == null)
        return RedirectToAction("Index");

    myAccount.PayIn(amount);
    _bank.SaveChanges();

    return RedirectToAction("Index");
}
[HttpGet]
[Authorize]
public ActionResult Withdraw(int account)
{
    return View(account);
}

[HttpPost]
[Authorize]
public RedirectToRouteResult Withdraw(int account, double amount)
{
    var myAccount = _bank.Accounts.FirstOrDefault(acc => acc.Number == account);
    if (myAccount != null)
    {
        if (!myAccount.Withdraw(amount))
            return RedirectToAction("Index");
    }
    else
        return RedirectToAction("Index");

    _bank.SaveChanges();
    return RedirectToAction("Index");
}

[HttpGet]
[Authorize]
public ActionResult Transfer(int account)
{
    return View(account);
}

[HttpPost]
[Authorize]
public RedirectToRouteResult Transfer(int from, int to, double amount)
{
    var sender = _bank.Accounts.FirstOrDefault(acc => acc.Number == from);
    var receiver = _bank.Accounts.FirstOrDefault(acc => acc.Number == to);
    if (sender != null && receiver != null)
    {
        if (!sender.Transfer(amount, receiver))
            return RedirectToAction("Index");
    }
    else
        return RedirectToAction("Index");

    _bank.SaveChanges();
    return RedirectToAction("Index");
}
```

Bei der Methode Transfer gibt es einen Unterschied: Hier wird auf zwei Konten zugegriffen, deswegen hat die Methode im Controller die Parameter _from_ und _to_, wobei _from_ wie sonst auch aus einem Hidden-Input-Feld im View stammt und _to_ vom Benutzer eingegeben wurde:

```csharp
@model int
@{    Layout = "~/Views/Shared/_Layout.cshtml";}

<h2>Überweisen</h2>

@using (Html.BeginForm("Transfer", "Home", FormMethod.Post, new { @class = "form-horizontal" }))
{
    <label for="amount">Betrag</label>
    @Html.TextBox("amount", "", new { @class = "form-control" })
    <br />
    <label for="to">Empfänger</label>
    @Html.TextBox("to", "", new { @class = "form-control" })
    @Html.Hidden("from", Model.ToString())
    <br />
    <input type="submit" value="Überweisen" class="btn btn-primary" />
}
```

## Erweiterung zum ViewModel

Die Methoden des Controllers übergeben ihrem zugehörigen View ein Model, zum Beispiel bei return View(accounts);. Das ist aber aktuell noch kein ViewModel, also keine eigens auf den View zugeschnittene Klasse. Dies werden wir nun ändern. Hintergrund ist, dass wir zwar im Controller wissen, ob die Aufrufe zum Einzahlen, Abheben und Überweisen funktioniert haben oder nicht, das aber dem Benutzer noch nicht anzeigen können. Wir fangen also damit an, beim Aufruf der Index-Methode eine Statusmeldung als optionalen Parameter zu übergeben:

```csharp
publicActionResult Index(string message = "", bool error = false, bool success = false)
```

Und in den aufrufenden Methoden wird ein anonymes Objekt erstellt, das diese Parameter enthält:

```csharp
return RedirectToAction("Index", new { message = "Das Konto Nummer " + accountNumber + "wurde erfolgreich angelegt.", success = true } );
```

Außerdem müssen wir für das ViewModel eine eigene Klasse erstellen:

```csharp
public ActionResult Index(string message = "", bool error = false, bool success = false)
```
Und in den aufrufenden Methoden wird ein anonymes Objekt erstellt, das diese Parameter enthält:
```csharp
return RedirectToAction("Index", new { message = "Das Konto Nummer " + accountNumber + "wurde erfolgreich angelegt.", success = true } );
```
Außerdem müssen wir für das ViewModel eine eigene Klasse erstellen:
```csharp
using System.Collections.Generic;

namespace MyBank.Models
{
    public class AccountOverviewViewModel
    {
        public List<BankAccount> Accounts { get; set; }
        public string Message { get; set; }
        public bool Error { get; set; }
        public bool Success { get; set; }
    }
}
```
Diese wird nun in der Index-Methode befüllt:
```csharp
[Authorize]
public ActionResult Index(string message = "", bool error = false, bool success = false)
{
    var userId = _userManager.FindById(User.Identity.GetUserId()).Id;

    var model = new AccountOverviewViewModel
    {
        Accounts = _bank.Accounts.Where(account => account.OwnerId == userId).ToList(),
        Message = message,
        Error = error,
        Success = success
    };
    
    return View(model);
}
```
Nun muss nur noch der View angepasst werden:
```csharp
@model MyBank.Models.AccountOverviewViewModel // andere Model-Klasse

<h2>Meine Konten</h2>

@if (Model.Error)
{
    <div class="alert alert-danger">@Model.Message</div>
}
@if(Model.Success)
{
    <div class="alert alert-success">@Model.Message</div> 
}

<table class="table">
    <tr>
        <td>Kontonummer</td>
        <td>Guthaben</td>
        <td>Aktion</td>
    </tr>
    @foreach (var bankAccount in Model.Accounts) // hier jetzt Model.Accounts!
    {
        …
``` 
		
Nun werden nach allen Aktionen Erfolgs- oder Fehlermeldungen ausgegeben:

![](https://raw.githubusercontent.com/JannikArndt/MyBank/master/Tutorial/image27.png)

# Beispielprojekt „MyBankAdmin"

WPF Windows-Programm mit Entity Framework 6

Dieses Projekt baut auf das ASP.NET MVC-Beispielprojekt „MyBank" auf und benutzt dessen Datenbank. Zunächst fügen wir der MyBank-Solution ein neues Projekt hinzu:

![](https://raw.githubusercontent.com/JannikArndt/MyBank/master/Tutorial/image28.png)

Ein WPF-Projekt auswählen:

![](https://raw.githubusercontent.com/JannikArndt/MyBank/master/Tutorial/image29.png)

Ein Fenster besteht aus zwei Dateien: der XAML-Datei für die Oberfläche und der xaml.cs für das Backend:

![](https://raw.githubusercontent.com/JannikArndt/MyBank/master/Tutorial/image30.png)

![](https://raw.githubusercontent.com/JannikArndt/MyBank/master/Tutorial/image31.png)

## Datenbankanbindung einfügen

Jetzt fügen wir die Datenquelle aus dem MVC-Beispielprojekt hinzu:

![](https://raw.githubusercontent.com/JannikArndt/MyBank/master/Tutorial/image32.png)

![](https://raw.githubusercontent.com/JannikArndt/MyBank/master/Tutorial/image33.png)

![](https://raw.githubusercontent.com/JannikArndt/MyBank/master/Tutorial/image34.png)

![](https://raw.githubusercontent.com/JannikArndt/MyBank/master/Tutorial/image35.png)

![](https://raw.githubusercontent.com/JannikArndt/MyBank/master/Tutorial/image36.png)

![](https://raw.githubusercontent.com/JannikArndt/MyBank/master/Tutorial/image37.png)

![](https://raw.githubusercontent.com/JannikArndt/MyBank/master/Tutorial/image38.png)

Die beiden generierten Klassen werden als edmx-Diagramm angezeigt und sind im Solution Explorer in dieser edmx-Datei zu finden:

![](https://raw.githubusercontent.com/JannikArndt/MyBank/master/Tutorial/image39.png)

In der Datei weist ein Hinweis darauf hin, dass diese Klasse generiert ist. Sie kann zwar beliebig verändert werden, sollte sie jedoch neu generiert werden, z.B. weil sich die Datenbank geändert hat, werden diese Änderungen überschrieben.

![](https://raw.githubusercontent.com/JannikArndt/MyBank/master/Tutorial/image40.png)

## Einträge anzeigen

Nun können wir in der MainWindow.xaml eine Oberfläche bauen, um die Daten aus der Datenbank anzeigen zu können. Ziel ist eine Liste aller Benutzer, wenn man einen Benutzer auswählt sollen Details und eine Auflistung seiner Konten angezeigt werden.

XAML-Dateien werden im Split-View angezeigt, oben eine Vorschau, unten der Code. Über den Reiter „Toolbox" kann man GUI-Elemente grafisch bearbeiten, um die volle Kontrolle zu behalten kann man die GUI aber auch im Code erstellen.

![](https://raw.githubusercontent.com/JannikArndt/MyBank/master/Tutorial/image41.png)

Das root-Element ist ein Window-Tag, darunter ist bereits ein Grid-Element vorgegeben. Wir unterteilen unser Grid in zwei Hälften, links die Kundenübersicht, rechts die Details.
```xml
<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="150" />
        <ColumnDefinition Width="\*" />
    </Grid.ColumnDefinitions>
    <!-- Links Kundenliste -->
    <!-- Rechts Details -->
</Grid>
```

Die Kundenliste ist ein `ListBox`-Element, die Detail-Ansicht ein weiteres `Grid` mit `Label`- und `TextBox`-Elementen. Für die Auflistung der Konten benutzen wir ein `DataGrid`-Element:
```xml
<ListBox Grid.Row="0" Grid.Column="0" Name="UsersListBox" Margin="10" />
<Grid Grid.Row="0" Grid.Column="1" Margin="10" Name="UserItemsGrid">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="100" />
        <ColumnDefinition Width="\*" />
    </Grid.ColumnDefinitions>
    <Grid.RowDefinitions>
        <RowDefinition Height="\*" />
        <RowDefinition Height="\*" />
        <RowDefinition Height="\*" />
        <RowDefinition Height="\*" />
        <RowDefinition Height="4\*" />
    </Grid.RowDefinitions>

    <Label Grid.Row="0" Grid.Column="0" Content="Id" />
    <TextBox Grid.Row="0" Grid.Column="1" Name="IdTextBox" Margin="5" />

    <Label Grid.Row="1" Grid.Column="0" Content="Name" />
    <TextBox Grid.Row="1" Grid.Column="1" Name="NameTextBox" Margin="5" />

    <Label Grid.Row="2" Grid.Column="0" Content="E-Mail" />
    <TextBox Grid.Row="2" Grid.Column="1" Name="EmailTextBox" Margin="5" />

    <Label Grid.Row="3" Grid.Column="0" Content="Telefonnummer" />
    <TextBox Grid.Row="3" Grid.Column="1" Name="PhoneTextBox" Margin="5" />

    <Label Grid.Row="4" Grid.Column="0" Content="Konten" />
    <DataGrid Grid.Row="4" Grid.Column="1" Name="AccountsDataGrid" Margin="5" />

</Grid>
``` 

Zunächst füllen wir die Benutzerliste aus dem Backend („Code Behind"). Dazu fügen wir der `MainWindow`-Klasse eine `Entities`-Property namens „Bank" hinzu, die unsere Datenbank repräsentiert. Im Konstruktor wird diese initialisiert und die Benutzer der Liste hinzugefügt:
```csharp
public partial class MainWindow : Window
{
    public Entities Bank { get; set; }

    public MainWindow()
    {
        InitializeComponent();
        Bank = newEntities();
        UsersListBox.ItemsSource = Bank.AspNetUsers.ToList();
    }
}
```

Zeit für einen ersten Test! Im Solution Explorer müssen das Projekt noch als StartUp Project festlegen:

![](https://raw.githubusercontent.com/JannikArndt/MyBank/master/Tutorial/image42.png)

Mit einem Klick auf „Start" erscheint das Programm:

![](https://raw.githubusercontent.com/JannikArndt/MyBank/master/Tutorial/image43.png)

 Die Benutzer werden in der Liste angezeigt, allerdings das, was die `ToString()`-Methode liefert. Dies lässt sich über das Attribut DisplayMemberPath kontrollieren:
```xml
<ListBox Grid.Row="0" Grid.Column="0" Name="UsersListBox" Margin="10" DisplayMemberPath="UserName" />
```
Als nächstes sollen die Details beim Klick auf einen Namen angezeigt werden. Wenn man im Designer auf die ListBox doppelklickt, dann wird dem XAML-Element das Attribut `SelectionChanged="UsersListBox\_SelectionChanged"` hinzugefügt. Die entsprechende Methode wird im Code Behind automatisch generiert. Als Parameter bekommt sie einen `sender` und ein Event. Das `sender`-Objekt ist die ListBox, von der man nach dem casten auch das `selectedItem` erhält:
```csharp
var user = (sender asListBox).SelectedItem asAspNetUser;
```
Die Properties des user-Objektes können nun den TextBoxen und dem AccountsDataGrid zugewiesen werden:
```csharp
private void UsersListBox\_SelectionChanged(object sender, SelectionChangedEventArgs e)
{
    var user = (sender asListBox).SelectedItem asAspNetUser;

    IdTextBox.Text = user.Id;
    NameTextBox.Text = user.UserName;
    EmailTextBox.Text = user.Email;
    PhoneTextBox.Text = user.PhoneNumber;

    AccountsDataGrid.ItemsSource = user.BankAccounts;
}
```
Das führt zu:

![](https://raw.githubusercontent.com/JannikArndt/MyBank/master/Tutorial/image44.png)

Standardmäßig wird im DataGrid für alle Properties eine Spalte generiert. Dies kann man mit `AutoGenerateColumns="False"` deaktivieren, muss dann aber die Spalten, die man behalten möchte, selbst definieren:
```xml
<DataGrid Grid.Row="4" Grid.Column="1" Name="AccountsDataGrid" Margin="5" AutoGenerateColumns="False">
    <DataGrid.Columns>
        <DataGridTextColumn Binding="{Binding Path=Number}" Header="Kontonummer" Width="\*" />
        <DataGridTextColumn Binding="{Binding Path=Balance, ConverterCulture='de-DE', StringFormat={}{0:C}}" Header="Saldo" Width="\*" />
    </DataGrid.Columns>
</DataGrid>
```

## Bindings zwischen Model und View

Dem DataGrid wird im Code Behind eine Liste als ItemsSource übergeben, im XAML-Code werden Spalten an bestimmte Properties gebunden, den Rest erstellt das Framework von alleine. Diese Bindings funktionieren jedoch nicht nur für einzelne Elemente, sondern auch für ganze Blöcke. Zum Beispiel können wir dem  Grid, in dem wir alle Details zum Benutzer anzeigen, das User-Objekt als Context übergeben. Die Zuweisung in der SelectionChanged-Methode besteht dann nur noch aus einer Zeile:
```csharp
UserItemsGrid.DataContext = (sender asListBox).SelectedItem asAspNetUser;
```
In der XAML-Datei bekommen die TextBox-Elemente ein Attribut `Text="{Binding UserName}"` und das DataGrid das Attribut `ItemsSource="{Binding BankAccounts}"`.

## Einträge bearbeiten

Wenn man einen Eintrag bearbeitet, einen anderen Benutzer auswählt und dann zum bearbeiteten Benutzer zurückkehrt, dann sieht man, dass die Änderung bleibt. Das liegt daran, dass die Textfelder per Bindings an die Objekte im Speicher gebunden sind und dieses Binding in beide Richtungen definiert ist. Allerdings werden die Änderungen nicht in der Datenbank persistiert.

Hierfür benötigt man aber nicht viel Code, es reicht, in die SelectionChanged-Methode die Zeile
```csharp
Bank.SaveChanges();
```
mit aufzunehmen.

Versucht man jedoch, einen Saldo via Doppelklick zu ändern, wird eine `InvalidOperationException` geworfen, mit der Info, dass „EditItem" für diese Ansicht nicht zulässig sei. Hintergrund ist, dass das Entity Framework in der Klasse AspNetUsers die Property
```csharp
public virtual ICollection<BankAccount> BankAccounts { get; set; }
```
generiert hat, die als HashSet initialisiert wird:
```csharp
this.BankAccounts = new HashSet<BankAccount>();
```
Ändert man diese beiden Einträge zu `List<BankAccount>` um, so wird das Bearbeiten plötzlich möglich.

Die Änderungen werden allerdings erst in die Datenbank persistiert, wenn man einen anderen Benutzer auswählt. Mit einem Doppelklick auf das `DataGrid` erzeugt man die Methode SelectionChanged, in der man, wie in der `SelectionChanged` der `ListBox`, ein Speichern anstoßen kann:
```csharp
private void AccountsDataGrid\_SelectionChanged(object sender, SelectionChangedEventArgs e)
{
    Bank.SaveChanges();
}
```
![](https://raw.githubusercontent.com/JannikArndt/MyBank/master/Tutorial/image45.png)

Damit ist auch die Erstellung der Admin-Software erfolgreich abgeschlossen.
