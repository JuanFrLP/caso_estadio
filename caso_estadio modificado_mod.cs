using System;
using System.IO;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BoletoEstadio 
{
    class Program 
    {
        static int CAPACIDAD = 5; 
        
        static int[] idOriginal = new int[CAPACIDAD]; 
        static int[] hashGenerado = new int[CAPACIDAD];
        static bool[] espacioOcupado = new bool[CAPACIDAD]; 
        static bool[] boletoValidado = new bool[CAPACIDAD];

        static void Main(string[] args) 
        {
            // configuración requerida por la librería QuestPDF
            QuestPDF.Settings.License = LicenseType.Community;

            bool ejecutando = true;

            while (ejecutando)
            {
                Console.WriteLine("\n--- MENU ESTADIO ---");
                Console.WriteLine("1. Generar boleto");
                Console.WriteLine("2. Validar boleto (Ingreso)");
                Console.WriteLine("3. Salir");
                Console.Write("Seleccione una opcion: ");
                
                string opcion = Console.ReadLine();
                Console.WriteLine();

                switch (opcion)
                {
                    case "1":
                        GenerarBoleto();
                        break;
                    case "2":
                        ValidarBoleto();
                        break;
                    case "3":
                        ejecutando = false;
                        break;
                    default:
                        Console.WriteLine("error, opcion invalida");
                        break;
                }
            }
        }

        static int CalcularHash8Digitos(int numero)
        {
            double calculo = (numero % 100000000) * 31 * Math.E * Math.PI; 
            long parteEntera = (long)Math.Truncate(calculo); 
            int hashFinal = (int)(Math.Abs(parteEntera) % 100000000); 
            return hashFinal;
        }

        static void GenerarBoleto()
        {
            Console.Write("Ingrese su numero de usuario/ID: ");
            if (!int.TryParse(Console.ReadLine(), out int id) || id < 0)
            {
                Console.WriteLine("error, ingrese un numero entero positivo");
                return;
            }

            int hash = CalcularHash8Digitos(id);
            int indiceInicio = hash % CAPACIDAD; 
            int indice = indiceInicio;

            // Búsqueda previa para evitar registros duplicados
            do 
            {
                if (espacioOcupado[indice] && idOriginal[indice] == id) 
                {
                    Console.WriteLine("error, no se pude generar duplicados");
                    return; 
                }

                if (!espacioOcupado[indice]) 
                {
                    break; 
                }
                
                indice = (indice + 1) % CAPACIDAD; 
                
            } while (indice != indiceInicio);

            indice = indiceInicio;
            bool insertado = false; 

            do
            {
                if (!espacioOcupado[indice])
                {
                    espacioOcupado[indice] = true;
                    idOriginal[indice] = id;
                    hashGenerado[indice] = hash;
                    boletoValidado[indice] = false;
                    insertado = true;
                    break;
                }
                
                indice = (indice + 1) % CAPACIDAD; 
                
            } while (indice != indiceInicio); 

            if (insertado)
            {
                // Llamada a la funcion del PDF
                GenerarBoletoPDF(id, hash);
                
                Console.WriteLine("boleto generado");
                Console.WriteLine("ID: " + id + " | Codigo: " + hash.ToString("D8"));
            }
            else
            {
                Console.WriteLine("error, limite de capacidad alcanzado"); 
            }
        }

        static void ValidarBoleto()
        {
            Console.Write("Ingrese ID del boleto a validar: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("error, ID invalido");
                return;
            }

            int hashEsperado = CalcularHash8Digitos(id);
            int indiceInicio = hashEsperado % CAPACIDAD;
            int indice = indiceInicio;

            bool encontrado = false;

            do
            {
                if (!espacioOcupado[indice])
                {
                    break; 
                }

                if (espacioOcupado[indice] && idOriginal[indice] == id)
                {
                    encontrado = true;
                    
                    if (boletoValidado[indice])
                    {
                        Console.WriteLine("error, el boleto ya fue validado previamente");
                    }
                    else
                    {
                        boletoValidado[indice] = true;
                        Console.WriteLine("ingreso exitoso");
                    }
                    break;
                }

                indice = (indice + 1) % CAPACIDAD;

            } while (indice != indiceInicio);

            if (!encontrado)
            {
                Console.WriteLine("error, boleto inexistente o no generado");
            }
        }

        static void GenerarBoletoPDF(int id, int hash)
        {
            string datosQR = "ID: " + id + " | HASH: " + hash.ToString("D8");

            byte[] qrImageBytes;
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(datosQR, QRCodeGenerator.ECCLevel.Q);
                using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
                {
                    qrImageBytes = qrCode.GetGraphic(10);
                }
            }

            string nombreArchivo = "Boleto_" + id + ".pdf";
            
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A6);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header()
                        .Text("Boleto de Ingreso - General")
                        .SemiBold().FontSize(16).FontColor(Colors.Black);

                    page.Content().Column(column =>
                    {
                        column.Spacing(10);
                        column.Item().Text("Valido para 1 persona");
                        column.Item().Text("ID de Usuario: " + id);
                        column.Item().Text("Codigo de Seguridad: " + hash.ToString("D8"));
                        
                        column.Item().Image(qrImageBytes);
                    });
                });
            })
            .GeneratePdf(nombreArchivo);
        }
    }
}