using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace GastroStock.Pages
{
    public class IngredienteModel
    {
        public int IdIngrediente { get; set; }
        public string Nombre { get; set; }
        public decimal StockActual { get; set; }
        public decimal StockMinimo { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string EstadoAlerta { get; set; }
    }

    public class InventarioModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public InventarioModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<IngredienteModel> ListaIngredientes { get; set; } = new List<IngredienteModel>();
        public int TotalAlertas { get; set; } = 0;

        public void OnGet()
        {
            string connectionString = _configuration.GetConnectionString("ConexionSQL");
            string query = "SELECT id_ingrediente, nombre, stock_actual, stock_minimo, fecha_vencimiento FROM Ingredientes";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    try
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var ingrediente = new IngredienteModel
                                {
                                    IdIngrediente = reader.GetInt32(0),
                                    Nombre = reader.GetString(1),
                                    StockActual = reader.GetDecimal(2),
                                    StockMinimo = reader.GetDecimal(3),
                                    FechaVencimiento = reader.GetDateTime(4)
                                };

                                int diasParaVencer = (ingrediente.FechaVencimiento - DateTime.Today).Days;

                                if (diasParaVencer <= 5)
                                {
                                    ingrediente.EstadoAlerta = "Por Vencer";
                                    TotalAlertas++;
                                }
                                else if (ingrediente.StockActual <= ingrediente.StockMinimo)
                                {
                                    ingrediente.EstadoAlerta = "Bajo Stock";
                                    TotalAlertas++;
                                }
                                else
                                {
                                    ingrediente.EstadoAlerta = "Óptimo";
                                }

                                ListaIngredientes.Add(ingrediente);
                            }
                        }
                    }
                    catch (Exception ex) { }
                }
            }
        }

        public IActionResult OnPostSimularVenta()
        {
            string connectionString = _configuration.GetConnectionString("ConexionSQL");

            // Resta inventario Y ADEMÁS inserta en la bitácora de Movimientos automáticamente asignado al usuario Administrador (ID 1)
            string query = @"
                UPDATE Ingredientes SET stock_actual = stock_actual - 1.00 WHERE id_ingrediente = 1 AND stock_actual >= 1;
                INSERT INTO Movimientos (id_ingrediente, id_usuario, tipo_movimiento, cantidad, motivo) 
                VALUES (1, 1, 'Salida', 1.00, 'Descuento por Venta Simulada');

                UPDATE Ingredientes SET stock_actual = stock_actual - 1.00 WHERE id_ingrediente = 2 AND stock_actual >= 1;
                INSERT INTO Movimientos (id_ingrediente, id_usuario, tipo_movimiento, cantidad, motivo) 
                VALUES (2, 1, 'Salida', 1.00, 'Descuento por Venta Simulada');
            ";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    try
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex) { }
                }
            }
            return RedirectToPage();
        }

        public IActionResult OnPostAgregarInsumo(string nombre, decimal stockActual, decimal stockMinimo, DateTime fechaVencimiento)
        {
            string connectionString = _configuration.GetConnectionString("ConexionSQL");

            // Registra el ingrediente y guarda en Movimientos que hubo una 'Entrada' de inventario
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string queryIngrediente = "INSERT INTO Ingredientes (nombre, stock_actual, stock_minimo, fecha_vencimiento) OUTPUT INSERTED.id_ingrediente VALUES (@Nombre, @StockActual, @StockMinimo, @FechaVencimiento)";
                    int nuevoId = 0;

                    using (SqlCommand command = new SqlCommand(queryIngrediente, connection))
                    {
                        command.Parameters.AddWithValue("@Nombre", nombre);
                        command.Parameters.AddWithValue("@StockActual", stockActual);
                        command.Parameters.AddWithValue("@StockMinimo", stockMinimo);
                        command.Parameters.AddWithValue("@FechaVencimiento", fechaVencimiento);
                        nuevoId = (int)command.ExecuteScalar();
                    }

                    string queryMovimiento = "INSERT INTO Movimientos (id_ingrediente, id_usuario, tipo_movimiento, cantidad, motivo) VALUES (@IdIngrediente, 1, 'Entrada', @Cantidad, 'Carga Inicial de Insumo')";
                    using (SqlCommand commandMov = new SqlCommand(queryMovimiento, connection))
                    {
                        commandMov.Parameters.AddWithValue("@IdIngrediente", nuevoId);
                        commandMov.Parameters.AddWithValue("@Cantidad", stockActual);
                        commandMov.ExecuteNonQuery();
                    }
                }
                catch (Exception ex) { }
            }
            return RedirectToPage();
        }
    }
}
