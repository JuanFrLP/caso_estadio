using System;

namespace BoletoEstadio
{
    class Program
    {
        static int CAPACIDAD = 100;
        
        static int[] idOriginal = new int[CAPACIDAD];
        static int[] hashGenerado = new int[CAPACIDAD];
        static bool[] espacioOcupado = new bool[CAPACIDAD];
        static bool[] boletoValidado = new bool[CAPACIDAD];

        static void Main(string[] args)
        {
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
            double calculo = numero * Math.E * Math.PI;
            
            long parteEntera = (long)Math.Truncate(calculo);
            
            int hashFinal = (int)(Math.Abs(parteEntera) % 100000000);
            
            return hashFinal;
        }

        static void GenerarBoleto()
        {
            Console.Write("Ingrese su numero de usuario/ID: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("error, no se pude generar");
                return;
            }

            int hash = CalcularHash8Digitos(id);
            
            int indiceInicio = hash % CAPACIDAD;
            int indice = indiceInicio;

            do
            {
                if (!espacioOcupado[indice]) 
                {
                    break; 
                }
                
                if (espacioOcupado[indice] && idOriginal[indice] == id)
                {
                    Console.WriteLine("error, no se pude generar");
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
                Console.WriteLine("boleto generado");
                Console.WriteLine("QR Data -> ID: " + id + " | HASH: " + hash.ToString("D8"));
            }
            else
            {
                Console.WriteLine("error, no se pude generar"); 
            }
        }

        static void ValidarBoleto()
        {
            Console.Write("Ingrese ID del boleto a validar: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("error, entrada invalida");
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
                Console.WriteLine("error, boleto no registrado");
            }
        }
    }
}