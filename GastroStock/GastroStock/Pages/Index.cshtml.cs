using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace GastroStock.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public IndexModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Esta propiedad es para que el archivo .cshtml no marque error
        [BindProperty]
        public string MensajeError { get; set; } = string.Empty;

        public void OnGet()
        {
        }

        public IActionResult OnPost(string usuario, string contrasenia)
        {
            string connectionString = _configuration.GetConnectionString("ConexionSQL");
            string query = "SELECT COUNT(1) FROM Usuarios WHERE usuario = @Usuario AND contrasenia = @Contrasenia";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Usuario", usuario ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Contrasenia", contrasenia ?? (object)DBNull.Value);

                    try
                    {
                        connection.Open();
                        int resultado = (int)command.ExecuteScalar();

                        if (resultado == 1)
                        {
                            return RedirectToPage("/Inventario");
                        }
                        else
                        {
                            MensajeError = "Usuario o contraseña incorrectos.";
                            return Page();
                        }
                    }
                    catch (Exception ex)
                    {
                        MensajeError = "Error de conexión: " + ex.Message;
                        return Page();
                    }
                }
            }
        }
    }
}
