using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ville;

namespace apiprojet;

public partial class MainPage : ContentPage
{
    private string gareDepUIC = string.Empty; // Code UIC pour la gare de départ
    private string gareArvUIC = string.Empty; // Code UIC pour la gare d'arrivée

    public MainPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    // Méthode pour rechercher les gares via ville ou nom de gare
    private async void OnSearchVilleButtonClicked(object sender, EventArgs e)
    {
        string query = ((Button)sender).CommandParameter.ToString() == "Dep" ? cityEntryDep.Text : cityEntryArv.Text;
        StackLayout dynamicContainer = ((Button)sender).CommandParameter.ToString() == "Dep" ? DynamicButtonsContainerDep : DynamicButtonsContainerArv;

        if (string.IsNullOrEmpty(query))
        {
            await DisplayAlert("Erreur", "Veuillez entrer une ville ou un nom de gare.", "OK");
            return;
        }

        Root response = await GetCityOrGareDataAsync(query);
        dynamicContainer.Children.Clear();

        if (response?.results != null && response.results.Any())
        {
            foreach (var result in response.results)
            {
                Button gareButton = new Button
                {
                    Text = $"{result.libelle}",
                    CommandParameter = result.code_uic,
                    HorizontalOptions = LayoutOptions.FillAndExpand
                };

                gareButton.Clicked += OnGareButtonClicked;
                dynamicContainer.Children.Add(gareButton);
            }
        }
        else
        {
            await DisplayAlert("Résultat", "Aucune gare trouvée pour cette recherche.", "OK");
        }
    }

    // Méthode pour gérer le clic sur une gare et sauvegarder son UIC
    private void OnGareButtonClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is string codeUIC)
        {
            if (button.Parent == DynamicButtonsContainerDep)
                gareDepUIC = codeUIC;
            else
                gareArvUIC = codeUIC;

            DisplayAlert("Gare sélectionnée", $"La gare a bien été sélectionnée.", "OK");
        }
    }

    // Méthode pour afficher le trajet avec l'API SNCF
    private async void OnSearchTrajetButtonClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(gareDepUIC) || string.IsNullOrEmpty(gareArvUIC))
        {
            await DisplayAlert("Erreur", "Veuillez sélectionner les gares de départ et d'arrivée.", "OK");
            return;
        }

        HttpClient client = new HttpClient();
        var authValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"272c0ef1-9ecb-4acc-a168-22e96ef262c8:"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);

        string url = $"https://api.sncf.com/v1/coverage/sncf/journeys?from=stop_area:SNCF:{gareDepUIC}&to=stop_area:SNCF:{gareArvUIC}";
        var response = await client.GetAsync(url);
        
        if (!response.IsSuccessStatusCode)
        {
            await DisplayAlert("Erreur", $"Erreur API : {response.StatusCode}", "OK");
            return;
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        Console.WriteLine("Réponse de l'API : " + jsonResponse);

        try
        {
            var jsonDoc = JsonDocument.Parse(jsonResponse);

            if (jsonDoc.RootElement.TryGetProperty("journeys", out var journeys) && journeys.GetArrayLength() > 0)
            {
                var journey = journeys[0];
                var duration = journey.GetProperty("duration").GetInt32();
                var carbon = journey.GetProperty("co2_emission").GetProperty("value").GetDouble();

                string departureDateTime = journey.GetProperty("departure_date_time").GetString();
                string arrivalDateTime = journey.GetProperty("arrival_date_time").GetString();

                string format = "yyyyMMdd'T'HHmmss";
                DateTime departureDate = DateTime.ParseExact(departureDateTime, format, CultureInfo.InvariantCulture);
                DateTime arrivalDate = DateTime.ParseExact(arrivalDateTime, format, CultureInfo.InvariantCulture);

                

                await DisplayAlert("Trajet trouvé",
                    $"Durée : {(duration > 0 ? TimeSpan.FromSeconds(duration).ToString() : "Non disponible")}\n" +
                    $"Empreinte carbone : {(carbon >= 0 ? carbon + " gCO2" : "Non disponible")}\n" +
                    $"Départ : {departureDate:dd/MM/yyyy HH:mm}\n" +
                    $"Arrivée : {arrivalDate:dd/MM/yyyy HH:mm}", "OK");
            }
            else
            {
                await DisplayAlert("Erreur", "Aucun trajet trouvé pour ce parcours.", "OK");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors du traitement de la réponse JSON : {ex.Message}");
            await DisplayAlert("Erreur", "Une erreur est survenue lors du traitement des données.", "OK");
        }
    }

    private async Task<Root> GetCityOrGareDataAsync(string query)
    {
        HttpClient client = new HttpClient();
        string url = $"https://ressources.data.sncf.com/api/explore/v2.1/catalog/datasets/liste-des-gares/records?refine=voyageurs%3A%22O%22&limit=50&where=libelle like \"%{query}%\" or commune like \"%{query}%\"";
        return await client.GetFromJsonAsync<Root>(url);
    }
}