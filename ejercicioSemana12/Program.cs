using System;
using System.Collections.Generic;
using System.Linq;

namespace ejercicioSemana12
{
    class Empleado
    {
        public string Nombre { get; set; }
        public List<Empleado> Subordinados { get; set; } = new List<Empleado>();
        public Empleado(string nombre) => Nombre = nombre;
        public override string ToString() => Nombre;
    }

    class Arbol
    {
        public Empleado Raiz { get; set; }

        public void Agregar(Empleado jefe, Empleado nuevo)
        {
            jefe.Subordinados.Add(nuevo);
        }

        public Empleado Buscar(string nombre, Empleado nodo = null)
        {
            if (nodo == null) nodo = Raiz;
            if (nodo == null) return null;
            if (nodo.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase)) return nodo;
            foreach (var sub in nodo.Subordinados)
            {
                var e = Buscar(nombre, sub);
                if (e != null) return e;
            }
            return null;
        }

        public void Mostrar(Empleado nodo = null, string s = "")
        {
            if (nodo == null) nodo = Raiz;
            if (nodo == null) return;
            Console.WriteLine(s + "└─ " + nodo);
            foreach (var sub in nodo.Subordinados) Mostrar(sub, s + "   ");
        }

        public int Contar(Empleado nodo = null)
        {
            if (nodo == null) nodo = Raiz;
            if (nodo == null) return 0;
            return 1 + nodo.Subordinados.Sum(Contar);
        }

        public int ObtenerNivel(Empleado buscado, Empleado actual = null, int nivel = 0)
        {
            if (actual == null) actual = Raiz;
            if (actual == null) return -1;
            if (actual == buscado) return nivel;
            foreach (var sub in actual.Subordinados)
            {
                int encontrado = ObtenerNivel(buscado, sub, nivel + 1);
                if (encontrado != -1) return encontrado;
            }
            return -1;
        }
    }

    class Edificio
    {
        public string Nombre { get; set; }
        public Edificio(string nombre) => Nombre = nombre;
        public override string ToString() => Nombre;
    }

    class Ruta
    {
        public Edificio A { get; set; }
        public Edificio B { get; set; }
        public int Distancia { get; set; }
        public Ruta(Edificio a, Edificio b, int d) => (A, B, Distancia) = (a, b, d);
    }

    class GrafoRutas
    {
        public List<Edificio> Edificios { get; set; } = new List<Edificio>();
        public List<Ruta> Rutas { get; set; } = new List<Ruta>();

        public void AgregarEdificio(string nombre)
        {
            if (Edificios.Any(e => e.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine($"→ '{nombre}' ya existe.");
                return;
            }
            Edificios.Add(new Edificio(nombre));
            Console.WriteLine($"→ Edificio '{nombre}' agregado.");
        }

        public void Conectar(string a, string b, int distancia)
        {
            var ea = Edificios.Find(e => e.Nombre.Equals(a, StringComparison.OrdinalIgnoreCase));
            var eb = Edificios.Find(e => e.Nombre.Equals(b, StringComparison.OrdinalIgnoreCase));
            if (ea == null || eb == null)
            {
                Console.WriteLine("→ Uno de los edificios no existe.");
                return;
            }
            if (Rutas.Any(r => (r.A == ea && r.B == eb) || (r.A == eb && r.B == ea)))
            {
                Console.WriteLine("→ Ya están conectados.");
                return;
            }
            Rutas.Add(new Ruta(ea, eb, distancia));
            Console.WriteLine($"→ {ea} ---{distancia}m---> {eb}");
        }

        public void Mostrar()
        {
            Console.WriteLine("\n--- RUTAS ---");
            foreach (var r in Rutas)
                Console.WriteLine($"{r.A} ---{r.Distancia}m---> {r.B}");
        }

        public bool EsConexo()
        {
            if (Edificios.Count == 0) return true;
            var visitados = new HashSet<Edificio>();
            DFS(Edificios[0], visitados);
            return visitados.Count == Edificios.Count;
        }

        private void DFS(Edificio actual, HashSet<Edificio> visitados)
        {
            visitados.Add(actual);
            foreach (var ruta in Rutas.Where(r => r.A == actual || r.B == actual))
            {
                var vecino = ruta.A == actual ? ruta.B : ruta.A;
                if (!visitados.Contains(vecino))
                    DFS(vecino, visitados);
            }
        }

        public (List<Edificio> camino, int distancia) RutaMasCorta(string inicio, string fin)
        {
            var ei = Edificios.Find(e => e.Nombre.Equals(inicio, StringComparison.OrdinalIgnoreCase));
            var ef = Edificios.Find(e => e.Nombre.Equals(fin, StringComparison.OrdinalIgnoreCase));
            if (ei == null || ef == null) return (null, -1);

            var dist = new Dictionary<Edificio, int>();
            var prev = new Dictionary<Edificio, Edificio>();
            var pq = new SortedSet<(int, Edificio)>();

            foreach (var e in Edificios) dist[e] = int.MaxValue;
            dist[ei] = 0;
            pq.Add((0, ei));

            while (pq.Count > 0)
            {
                var (d, actual) = pq.Min; pq.Remove(pq.Min);
                if (actual == ef) break;

                foreach (var ruta in Rutas.Where(r => r.A == actual || r.B == actual))
                {
                    var vecino = ruta.A == actual ? ruta.B : ruta.A;
                    int nueva = dist[actual] + ruta.Distancia;
                    if (nueva < dist[vecino])
                    {
                        dist[vecino] = nueva;
                        prev[vecino] = actual;
                        pq.Add((nueva, vecino));
                    }
                }
            }

            if (dist[ef] == int.MaxValue) return (null, -1);

            var camino = new List<Edificio>();
            var nodo = ef;
            while (nodo != null)
            {
                camino.Add(nodo);
                nodo = prev.ContainsKey(nodo) ? prev[nodo] : null;
            }
            camino.Reverse();
            return (camino, dist[ef]);
        }
    }

    internal class Program
    {
        static Arbol arbol = new Arbol();
        static GrafoRutas grafo = new GrafoRutas();
        static Dictionary<string, Empleado> empleados = new Dictionary<string, Empleado>(StringComparer.OrdinalIgnoreCase);

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("=== INNOVATEC - ÁRBOL Y GRAFO ===\n");
            MenuPrincipal();
        }

        static void MenuPrincipal()
        {
            while (true)
            {
                Console.WriteLine("════════════════════════════════");
                Console.WriteLine("1. Árbol: Jerarquía");
                Console.WriteLine("2. Grafo: Rutas");
                Console.WriteLine("3. Salir");
                Console.Write("→ ");

                switch (Console.ReadLine().Trim())
                {
                    case "1": MenuArbol(); break;
                    case "2": MenuGrafo(); break;
                    case "3": return;
                    default: Console.WriteLine("→ Opción inválida"); break;
                }
                Pausa();
            }
        }

        static void MenuArbol()
        {
            Console.WriteLine("\n--- ÁRBOL: AGREGAR EMPLEADOS ---");
            while (true)
            {
                Console.WriteLine("──────────────────────────────────");
                Console.Write("Nombre (fin para terminar): ");
                string nombre = Console.ReadLine().Trim();
                if (nombre.Equals("fin", StringComparison.OrdinalIgnoreCase)) break;

                Console.Write("Jefe: ");
                string jefe = Console.ReadLine().Trim();

                if (empleados.ContainsKey(nombre))
                {
                    Console.WriteLine("→ Ya existe.");
                    continue;
                }

                var nuevo = new Empleado(nombre);
                empleados[nombre] = nuevo;

                if (string.IsNullOrEmpty(jefe))
                {
                    if (arbol.Raiz != null) { Console.WriteLine("→ Ya hay Director/a."); empleados.Remove(nombre); continue; }
                    arbol.Raiz = nuevo;
                    Console.WriteLine($"→ {nombre} → DIRECTOR/A");
                }
                else
                {
                    if (!empleados.ContainsKey(jefe)) { Console.WriteLine($"→ '{jefe}' no existe. Agrégalo primero."); empleados.Remove(nombre); continue; }
                    arbol.Agregar(empleados[jefe], nuevo);
                    Console.WriteLine($"→ {nombre} → reporta a {jefe}");
                }
            }

            Console.WriteLine("\n--- JERARQUÍA ---");
            arbol.Mostrar();
            Console.WriteLine($"→ Total: {arbol.Contar()} empleados");

            Console.Write("\nNivel de quién? ");
            string quien = Console.ReadLine().Trim();
            var emp = arbol.Buscar(quien);
            if (emp != null)
                Console.WriteLine($"→ Nivel de {quien}: {arbol.ObtenerNivel(emp)} (0 = Director/a)");
            else
                Console.WriteLine("→ No encontrado");
        }

        static void MenuGrafo()
        {
            Console.WriteLine("\n--- GRAFO: EDIFICIOS Y RUTAS ---");

            Console.WriteLine("Ingresa edificios (escribe 'fin' para terminar):");
            while (true)
            {
                Console.Write("Edificio: ");
                string nombre = Console.ReadLine().Trim();
                if (nombre.Equals("fin", StringComparison.OrdinalIgnoreCase)) break;
                grafo.AgregarEdificio(nombre);
            }

            Console.WriteLine("\nConecta edificios (A, B, distancia):");
            while (true)
            {
                Console.Write("Conexión (fin para terminar): ");
                string entrada = Console.ReadLine().Trim();
                if (entrada.Equals("fin", StringComparison.OrdinalIgnoreCase)) break;

                var partes = entrada.Split(',');
                if (partes.Length != 3) { Console.WriteLine("→ Formato: A, B, 100"); continue; }

                string a = partes[0].Trim(), b = partes[1].Trim();
                if (!int.TryParse(partes[2].Trim(), out int d)) { Console.WriteLine("→ Distancia inválida"); continue; }

                grafo.Conectar(a, b, d);
            }

            grafo.Mostrar();
            Console.WriteLine($"→ ¿Conexo? {(grafo.EsConexo() ? "SÍ" : "NO")}");

            Console.Write("\nRuta más corta (inicio): ");
            string ini = Console.ReadLine().Trim();
            Console.Write("Ruta más corta (fin): ");
            string fin = Console.ReadLine().Trim();

            var (camino, dist) = grafo.RutaMasCorta(ini, fin);
            if (camino != null)
                Console.WriteLine($"\n→ Ruta: {string.Join(" → ", camino.Select(e => e.Nombre))} | {dist}m");
            else
                Console.WriteLine("→ No hay ruta.");
        }

        static void Pausa()
        {
            Console.WriteLine("\nPresiona Enter para continuar...");
            Console.ReadLine();
            Console.Clear();
        }
    }
}