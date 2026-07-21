class Program
{
    // Estadísticas acumuladas durante la ejecución
    static int totalValidas = 0;
    static int totalInvalidas = 0;
    static readonly Dictionary<string, int> conteoPorMarca = new();
    static readonly Random rng = new();

    static void Main()
    {
        bool continuar = true;

        do
        {
            MostrarMenu();
            string opcion = Console.ReadLine() ?? "";

            try
            {
                switch (opcion)
                {
                    case "1":
                        OpcionValidarTarjeta();
                        break;
                    case "2":
                        OpcionValidarDesdeArchivo();
                        break;
                    case "3":
                        OpcionGenerarNumero();
                        break;
                    case "4":
                        MostrarEstadisticas();
                        break;
                    case "5":
                        continuar = false;
                        Console.WriteLine("Saliendo...");
                        break;
                    default:
                        Console.WriteLine("Opción inválida.");
                        break;
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Error: el archivo especificado no existe.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ocurrió un error: {ex.Message}");
            }

            Console.WriteLine();
        } while (continuar);
    }

    static void MostrarMenu()
    {
        Console.WriteLine("=== VALIDADOR DE TARJETAS ===");
        Console.WriteLine("1. Validar una tarjeta");
        Console.WriteLine("2. Validar desde archivo");
        Console.WriteLine("3. Generar número válido");
        Console.WriteLine("4. Estadísticas");
        Console.WriteLine("5. Salir");
        Console.Write("Seleccione una opción: ");
    }

    static void OpcionValidarTarjeta()
    {
        Console.Write("Ingrese el número de tarjeta: ");
        string entrada = Console.ReadLine() ?? "";

        if (!EsEntradaValida(entrada))
        {
            Console.WriteLine("Error: el número debe contener solo dígitos y no puede estar vacío.");
            return;
        }

        string numero = Limpiar(entrada);
        bool esValida = ValidarTarjeta(numero);
        string marca = IdentificarMarca(numero);

        Console.WriteLine($"Número: {numero}");
        Console.WriteLine($"Marca: {marca}");
        Console.WriteLine(esValida ? "Estado: ✅ VÁLIDA" : "Estado: ❌ INVÁLIDA");

        RegistrarEstadistica(esValida, marca);
    }

    static void OpcionValidarDesdeArchivo()
    {
        Console.Write("Ingrese la ruta del archivo: ");
        string ruta = Console.ReadLine() ?? "";
        ValidarDesdeArchivo(ruta);
    }

    static void OpcionGenerarNumero()
    {
        string numero = GenerarNumeroValido();
        string marca = IdentificarMarca(numero);

        Console.WriteLine($"Número generado: {numero}");
        Console.WriteLine($"Marca: {marca}");
    }

    // Limpia espacios y guiones que el usuario pueda incluir en la entrada
    static string Limpiar(string numero)
    {
        return numero.Replace(" ", "").Replace("-", "");
    }

    // Valida que la entrada (ya limpiada) sea no vacía y contenga solo dígitos
    static bool EsEntradaValida(string numero)
    {
        numero = Limpiar(numero);

        if (string.IsNullOrEmpty(numero))
        {
            return false;
        }

        foreach (char c in numero)
        {
            if (!char.IsDigit(c))
            {
                return false;
            }
        }

        return true;
    }

    static bool ValidarTarjeta(string numero)
    {
        numero = Limpiar(numero);
        return CalcularSumaLuhn(numero) % 10 == 0;
    }

    // Núcleo del algoritmo de Luhn: invierte el número y duplica los dígitos en posición par (1-indexed)
    static int CalcularSumaLuhn(string numero)
    {
        char[] caracteres = numero.ToCharArray();
        Array.Reverse(caracteres);
        string invertido = new(caracteres);

        int suma = 0;
        for (int i = 0; i < invertido.Length; i++)
        {
            int digito = (int)char.GetNumericValue(invertido[i]);
            int posicion = i + 1;

            if (posicion % 2 == 0)
            {
                digito *= 2;
                if (digito >= 10)
                {
                    digito -= 9;
                }
            }

            suma += digito;
        }

        return suma;
    }

    static string IdentificarMarca(string numero)
    {
        numero = Limpiar(numero);
        int len = numero.Length;

        if (len == 15 && (numero.StartsWith("34") || numero.StartsWith("37")))
        {
            return "American Express";
        }

        if ((len == 13 || len == 16) && numero.StartsWith('4'))
        {
            return "Visa";
        }

        if (len == 16)
        {
            int prefijo2 = int.Parse(numero[..2]);
            if (prefijo2 >= 51 && prefijo2 <= 55)
            {
                return "Mastercard";
            }
        }

        if (len >= 16 && len <= 19)
        {
            if (numero.StartsWith("6011") || numero.StartsWith("65"))
            {
                return "Discover";
            }

            if (len >= 3)
            {
                int prefijo3 = int.Parse(numero[..3]);
                if (prefijo3 >= 644 && prefijo3 <= 649)
                {
                    return "Discover";
                }
            }

            if (len >= 6)
            {
                int prefijo6 = int.Parse(numero[..6]);
                if (prefijo6 >= 622126 && prefijo6 <= 622925)
                {
                    return "Discover";
                }
            }
        }

        return "Desconocida";
    }

    static void ValidarDesdeArchivo(string ruta)
    {
        string[] lineas = File.ReadAllLines(ruta);
        int validas = 0;
        int invalidas = 0;

        foreach (string linea in lineas)
        {
            string entrada = linea.Trim();
            if (string.IsNullOrWhiteSpace(entrada))
            {
                continue;
            }

            if (!EsEntradaValida(entrada))
            {
                Console.WriteLine($"Número: {entrada}");
                Console.WriteLine("Estado: ❌ INVÁLIDA (formato incorrecto)");
                Console.WriteLine();
                invalidas++;
                continue;
            }

            string numero = Limpiar(entrada);
            bool esValida = ValidarTarjeta(numero);
            string marca = IdentificarMarca(numero);

            Console.WriteLine($"Número: {numero}");
            Console.WriteLine($"Marca: {marca}");
            Console.WriteLine(esValida ? "Estado: ✅ VÁLIDA" : "Estado: ❌ INVÁLIDA");
            Console.WriteLine();

            if (esValida) validas++; else invalidas++;
            RegistrarEstadistica(esValida, marca);
        }

        Console.WriteLine("=== RESUMEN ===");
        Console.WriteLine($"Total procesadas: {validas + invalidas}");
        Console.WriteLine($"Válidas: {validas}");
        Console.WriteLine($"Inválidas: {invalidas}");
    }

    static string GenerarNumeroValido()
    {
        (string Prefijo, int Longitud)[] plantillas =
        {
            ("4", 16),
            ("4", 13),
            ("51", 16),
            ("37", 15),
            ("6011", 16),
        };

        (string prefijo, int longitud) = plantillas[rng.Next(plantillas.Length)];
        string cuerpo = prefijo;

        for (int i = cuerpo.Length; i < longitud - 1; i++)
        {
            cuerpo += rng.Next(0, 10).ToString();
        }

        // Se calcula matemáticamente el dígito final para que la suma de Luhn sea múltiplo de 10
        int sumaParcial = CalcularSumaLuhn(cuerpo + "0");
        int digitoFinal = (10 - (sumaParcial % 10)) % 10;

        return cuerpo + digitoFinal;
    }

    static void RegistrarEstadistica(bool esValida, string marca)
    {
        if (esValida)
        {
            totalValidas++;
        }
        else
        {
            totalInvalidas++;
        }

        if (conteoPorMarca.TryGetValue(marca, out int actual))
        {
            conteoPorMarca[marca] = actual + 1;
        }
        else
        {
            conteoPorMarca[marca] = 1;
        }
    }

    static void MostrarEstadisticas()
    {
        Console.WriteLine("=== ESTADÍSTICAS ===");
        Console.WriteLine($"Total procesadas: {totalValidas + totalInvalidas}");
        Console.WriteLine($"Válidas: {totalValidas}");
        Console.WriteLine($"Inválidas: {totalInvalidas}");
        Console.WriteLine("Desglose por marca:");

        foreach (var par in conteoPorMarca)
        {
            Console.WriteLine($"  {par.Key}: {par.Value}");
        }
    }
}
