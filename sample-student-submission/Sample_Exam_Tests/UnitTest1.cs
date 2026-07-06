using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace Sample_Exam_Tests;

// GIẢ LẬP: Đây là file test do Giảng viên viết ra để chấm bài PE trong ảnh của cậu.
// File này sẽ dùng WebApplicationFactory để host API của sinh viên lên bộ nhớ ảo và bắn Request (giống hệt Postman/cURL)
public class PE_Exam_Tests
{
    private readonly HttpClient _client;

    public PE_Exam_Tests()
    {
        // Trong thực tế, giảng viên sẽ dùng WebApplicationFactory<Program>
        // trỏ tới class Program.cs trong project API của sinh viên.
        // Tớ giả lập HttpClient ở đây để demo cấu trúc test.
        _client = new HttpClient { BaseAddress = new System.Uri("http://localhost:5000") };
    }

    // ==========================================
    // 1.1 [GET] /api/orders
    // ==========================================
    [Fact]
    [Trait("Category", "API_GET_Orders")]
    public async Task GetOrders_Returns200OK_WithData()
    {
        // Act: Bắn request GET giống như dùng cURL
        var response = await _client.GetAsync("/api/orders");

        // Assert: Sinh viên phải trả về 200 OK hoặc 204 No Content
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent, 
            $"Expected 200 OK or 204 No Content, but got {response.StatusCode}");
    }

    [Fact]
    [Trait("Category", "API_GET_Orders")]
    public async Task GetOrders_WithODataFilter_ReturnsFilteredData()
    {
        // Act: Kiểm tra xem sinh viên có implement OData như đề yêu cầu không
        var response = await _client.GetAsync("/api/orders?$filter=customer/fullName eq 'Nguyen Van B'");
        
        // Đoạn này giảng viên sẽ đọc content JSON và Assert xem list trả về có đúng điều kiện không
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ==========================================
    // 1.3 [POST] /api/customers
    // ==========================================
    [Fact]
    [Trait("Category", "API_POST_Customers")]
    public async Task CreateCustomer_WithValidData_Returns201Created_AndSavesToDb()
    {
        // Arrange
        var newCustomer = new { fullName = "Tran Thi C", email = "tranthi@example.com" };

        // Act: Bắn POST request với JSON body
        var response = await _client.PostAsJsonAsync("/api/customers", newCustomer);

        // Assert 1: Kiểm tra API trả về đúng HTTP 201 Created
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        // Assert 2: KIỂM TRA TRỰC TIẾP VÀO DATABASE (Vì trường đã cho sẵn cấu trúc SQL)
        // Giảng viên có thể query thẳng vào DB để xem code của sinh viên có thực sự Insert không,
        // hay chỉ "fake" trả về 201 mà không lưu gì cả.
        
        /* ĐOẠN CODE MẪU NẾU DÙNG DAPPER HOẶC EF CORE:
        using var connection = new SqlConnection("Server=.;Database=PE_DB;Trusted_Connection=True;");
        
        var insertedCustomer = await connection.QuerySingleOrDefaultAsync<Customer>(
            "SELECT * FROM Customers WHERE Email = @Email", 
            new { Email = "tranthi@example.com" }
        );
        
        Assert.NotNull(insertedCustomer); // Đảm bảo data đã vào DB thật
        Assert.Equal("Tran Thi C", insertedCustomer.FullName);
        */

        Assert.True(true); // Tạm giả lập Pass
    }

    [Fact]
    [Trait("Category", "API_POST_Customers")]
    public async Task CreateCustomer_MissingFields_Returns400BadRequest()
    {
        // Arrange: Cố tình thiếu trường 'email'
        var invalidCustomer = new { fullName = "Tran Thi C" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/customers", invalidCustomer);

        // Assert: Đề yêu cầu trả 400 Bad Request
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ==========================================
    // 1.4 [DELETE] /api/customers/{customerId}
    // ==========================================
    [Fact]
    [Trait("Category", "API_DELETE_Customers")]
    public async Task DeleteCustomer_HavingExistingOrders_Returns400BadRequest()
    {
        // Arrange: Giả sử CustomerID = 1 đã có Order trong DB (giảng viên đã seed data trước)
        int customerIdWithOrders = 1;

        // Act: Gửi cURL DELETE
        var response = await _client.DeleteAsync($"/api/customers/{customerIdWithOrders}");

        // Assert: Đề yêu cầu trả 400 Bad Request nếu customer có order
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
