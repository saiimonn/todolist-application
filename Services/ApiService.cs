using System.Globalization;
using System.Text;
using System.Text.Json;
using todolist_application.Models;

namespace todolist_application.Services;

public static class ApiService
{
    private static readonly HttpClient Http = new()
    {
        BaseAddress = new Uri("https://todo-list.dcism.org/")
    };

    // ─── Auth ────────────────────────────────────────────────────────────────

    public static async Task<(bool IsSuccess, string Message)> SignUpAsync(
        string firstName,
        string lastName,
        string email,
        string password,
        string confirmPassword)
    {
        // POST with JSON body – server reads via json_decode(file_get_contents('php://input'))
        var payload = new
        {
            first_name = firstName,
            last_name = lastName,
            email = email,
            password = password,
            confirm_password = confirmPassword
        };
        var response = await SendJsonPostAsync("signup_action.php", payload);
        return (response.Status == 200, response.Message);
    }

    public static async Task<(bool IsSuccess, string Message, UserInfo? User)> SignInAsync(
        string email,
        string password)
    {
        // GET with query string
        var route = $"signin_action.php?email={Uri.EscapeDataString(email)}&password={Uri.EscapeDataString(password)}";
        var parsed = await SendGetAsync(route);

        if (parsed.Status != 200)
            return (false, parsed.Message, null);

        if (string.IsNullOrWhiteSpace(parsed.RawData))
            return (false, "Unable to read account details from server response.", null);

        var user = ParseUser(parsed.RawData);
        return user is null
            ? (false, "Unable to read account details from server response.", null)
            : (true, parsed.Message, user);
    }

    // ─── Items ───────────────────────────────────────────────────────────────

    public static async Task<(bool IsSuccess, string Message, List<ToDoClass> Items)> GetItemsAsync(
        string status,
        int userId)
    {
        // GET with query string
        var route = $"getItems_action.php?status={Uri.EscapeDataString(status)}&user_id={userId}";
        var parsed = await SendGetAsync(route);

        if (parsed.Status != 200)
            return (false, parsed.Message, new List<ToDoClass>());

        var items = ParseItems(parsed.RawData);
        return (true, parsed.Message, items);
    }

    public static async Task<(bool IsSuccess, string Message, ToDoClass? Item)> AddItemAsync(
        string itemName,
        string itemDescription,
        int userId)
    {
        // POST with JSON body – consistent with how the PHP backend reads the payload
        var payload = new
        {
            item_name = itemName,
            item_description = itemDescription,
            user_id = userId
        };
        var response = await SendJsonPostAsync("addItem_action.php", payload);

        if (response.Status != 200)
            return (false, response.Message, null);

        // Try to use the item the server echoes back; if it can't be parsed,
        // build one locally so the UI always reflects the new task.
        var item = string.IsNullOrWhiteSpace(response.RawData)
            ? null
            : ParseItem(response.RawData);

        item ??= new ToDoClass
        {
            item_name = itemName,
            item_description = itemDescription,
            status = "active",
            user_id = userId
        };

        return (true, response.Message, item);
    }

    public static async Task<(bool IsSuccess, string Message)> UpdateItemAsync(
        int itemId,
        string itemName,
        string itemDescription)
    {
        // PUT – PHP does NOT populate $_POST for PUT; must send JSON and read php://input
        var payload = new
        {
            item_id = itemId,
            item_name = itemName,
            item_description = itemDescription
        };
        var response = await SendJsonPutAsync("editItem_action.php", payload);
        return (response.Status == 200, response.Message);
    }

    public static async Task<(bool IsSuccess, string Message)> UpdateStatusAsync(
        int itemId,
        string status)
    {
        // PUT – same reasoning: send JSON
        var payload = new
        {
            item_id = itemId,
            status = status
        };
        var response = await SendJsonPutAsync("statusItem_action.php", payload);
        return (response.Status == 200, response.Message);
    }

    public static async Task<(bool IsSuccess, string Message)> DeleteItemAsync(int itemId)
    {
        // DELETE with query string – correct as-is
        var response = await Http.DeleteAsync($"deleteItem_action.php?item_id={itemId}");
        var content = await response.Content.ReadAsStringAsync();
        var parsed = ParseApiResponse(content, response.IsSuccessStatusCode);
        return (parsed.Status == 200, parsed.Message);
    }

    // ─── HTTP helpers ────────────────────────────────────────────────────────

    /// <summary>GET request, returns parsed API response.</summary>
    private static async Task<ApiResponse> SendGetAsync(string route)
    {
        var response = await Http.GetAsync(route);
        var content = await response.Content.ReadAsStringAsync();
        return ParseApiResponse(content, response.IsSuccessStatusCode);
    }

    /// <summary>
    /// POST with application/x-www-form-urlencoded body.
    /// PHP reads this via $_POST – suitable for signup and addItem.
    /// </summary>
    private static async Task<ApiResponse> SendFormPostAsync(
        string route,
        Dictionary<string, string> fields)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, route)
        {
            Content = new FormUrlEncodedContent(fields)
        };
        var response = await Http.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        return ParseApiResponse(content, response.IsSuccessStatusCode);
    }

    /// <summary>
    /// POST with application/json body.
    /// Use when the PHP endpoint reads the body via json_decode(file_get_contents('php://input')).
    /// </summary>
    private static async Task<ApiResponse> SendJsonPostAsync(string route, object payload)
    {
        var json = JsonSerializer.Serialize(payload);
        using var request = new HttpRequestMessage(HttpMethod.Post, route)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        var response = await Http.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        return ParseApiResponse(content, response.IsSuccessStatusCode);
    }

    /// <summary>
    /// PUT with application/json body.
    /// PHP reads this via json_decode(file_get_contents('php://input')) – required for PUT/PATCH.
    /// </summary>
    private static async Task<ApiResponse> SendJsonPutAsync(string route, object payload)
    {
        var json = JsonSerializer.Serialize(payload);
        using var request = new HttpRequestMessage(HttpMethod.Put, route)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        var response = await Http.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        return ParseApiResponse(content, response.IsSuccessStatusCode);
    }

    // ─── JSON parsing ────────────────────────────────────────────────────────

    private static ApiResponse ParseApiResponse(string content, bool isHttpSuccess)
    {
        if (string.IsNullOrWhiteSpace(content))
            return new ApiResponse(isHttpSuccess ? 200 : 500,
                                   isHttpSuccess ? "Success" : "No response from API.", null);
        try
        {
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            var status = ReadInt(root, "status") ?? (isHttpSuccess ? 200 : 500);
            var message = ReadString(root, "message");

            if (string.IsNullOrWhiteSpace(message))
                message = status == 200 ? "Success" : "Request failed.";

            string? rawData = null;
            if (TryGetProperty(root, "data", out var data))
                rawData = data.GetRawText();

            return new ApiResponse(status, message, rawData);
        }
        catch
        {
            return new ApiResponse(isHttpSuccess ? 200 : 500,
                                   isHttpSuccess ? "Success" : "Unable to parse API response.", null);
        }
    }

    private static UserInfo? ParseUser(string rawData)
    {
        try
        {
            using var doc = JsonDocument.Parse(rawData);
            var userEl = doc.RootElement;
            if (userEl.ValueKind != JsonValueKind.Object) return null;

            return new UserInfo
            {
                id = ReadInt(userEl, "id") ?? 0,
                fname = ReadString(userEl, "fname") ?? string.Empty,
                lname = ReadString(userEl, "lname") ?? string.Empty,
                email = ReadString(userEl, "email") ?? string.Empty,
                timemodified = ReadString(userEl, "timemodified") ?? string.Empty
            };
        }
        catch { return null; }
    }

    private static List<ToDoClass> ParseItems(string? rawData)
    {
        var items = new List<ToDoClass>();
        if (string.IsNullOrWhiteSpace(rawData)) return items;

        try
        {
            using var doc = JsonDocument.Parse(rawData);
            var data = doc.RootElement;

            if (data.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in data.EnumerateObject())
                {
                    if (property.Value.ValueKind != JsonValueKind.Object) continue;
                    var item = ParseItem(property.Value.GetRawText());
                    if (item is not null) items.Add(item);
                }
            }
            else if (data.ValueKind == JsonValueKind.Array)
            {
                foreach (var entry in data.EnumerateArray())
                {
                    var item = ParseItem(entry.GetRawText());
                    if (item is not null) items.Add(item);
                }
            }
        }
        catch { /* return whatever was collected */ }

        return items;
    }

    private static ToDoClass? ParseItem(string rawJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(rawJson);
            var itemEl = doc.RootElement;
            if (itemEl.ValueKind != JsonValueKind.Object) return null;

            return new ToDoClass
            {
                item_id = ReadInt(itemEl, "item_id") ?? 0,
                item_name = ReadString(itemEl, "item_name") ?? string.Empty,
                item_description = ReadString(itemEl, "item_description") ?? string.Empty,
                status = ReadString(itemEl, "status") ?? string.Empty,
                user_id = ReadInt(itemEl, "user_id") ?? 0
            };
        }
        catch { return null; }
    }

    // ─── Element helpers ─────────────────────────────────────────────────────

    private static bool TryGetProperty(JsonElement element, string name, out JsonElement value)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }
        value = default;
        return false;
    }

    private static int? ReadInt(JsonElement element, string propertyName)
    {
        if (!TryGetProperty(element, propertyName, out var value)) return null;

        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var n)) return n;

        if (value.ValueKind == JsonValueKind.String &&
            int.TryParse(value.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var s))
            return s;

        return null;
    }

    private static string? ReadString(JsonElement element, string propertyName)
    {
        if (!TryGetProperty(element, propertyName, out var value)) return null;

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number => value.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => null
        };
    }

    private readonly record struct ApiResponse(int Status, string Message, string? RawData);
}