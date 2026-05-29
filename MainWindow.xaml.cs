using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Text;
using System.ComponentModel;

namespace AnalizadorLexico
{
    public enum TipoToken
    {
        PalabraReservada, Identificador, ConstanteNumerica, Cadena,
        OperadorAritmetico, OperadorRelacional, OperadorLogico,
        OperadorAsignacion, SimboloEspecial, Blanco, ErrorLexico
    }

    public class Token
    {
        public TipoToken Tipo { get; }
        public string Lexema { get; }
        public int Linea { get; }
        public int Columna { get; }

        public Token(TipoToken tipo, string lexema, int linea, int columna)
        {
            Tipo = tipo;
            Lexema = lexema;
            Linea = linea;
            Columna = columna;
        }
    }

    public class TokenItem
    {
        public int Numero { get; set; }
        public string Lexema { get; set; }
        public string Tipo { get; set; }
        public int Linea { get; set; }
        public int Columna { get; set; }
        public Brush Color { get; set; }
        public bool EsError { get; set; }
    }

    public class Lexer
    {
        private static readonly HashSet<string> Reservadas = new HashSet<string>
            { "if", "else", "const", "var", "bool", "true", "false", "print", "void", "return", "public" };

        private readonly string _src;
        private int _pos, _linea, _col;

        public Lexer(string fuente)
        {
            _src = fuente;
            _pos = 0;
            _linea = 1;
            _col = 1;
        }

        public List<Token> Analizar()
        {
            var lista = new List<Token>();
            while (_pos < _src.Length)
                lista.Add(Siguiente());
            return lista;
        }

        private Token Siguiente()
        {
            int l = _linea, c = _col;
            char ch = Cur();

            if (char.IsWhiteSpace(ch))
            {
                var sb = new StringBuilder();
                while (_pos < _src.Length && char.IsWhiteSpace(Cur()))
                {
                    sb.Append(Cur());
                    Adv();
                }
                return new Token(TipoToken.Blanco, sb.ToString(), l, c);
            }

            if (ch == '"')
            {
                var sb = new StringBuilder();
                sb.Append(ch);
                Adv();
                while (_pos < _src.Length && Cur() != '"')
                {
                    if (Cur() == '\\' && _pos + 1 < _src.Length)
                    {
                        sb.Append(Cur());
                        Adv();
                    }
                    sb.Append(Cur());
                    Adv();
                }
                if (_pos < _src.Length)
                {
                    sb.Append('"');
                    Adv();
                    return new Token(TipoToken.Cadena, sb.ToString(), l, c);
                }
                return new Token(TipoToken.ErrorLexico, sb.ToString(), l, c);
            }

            if (char.IsDigit(ch))
            {
                var sb = new StringBuilder();
                while (_pos < _src.Length && char.IsDigit(Cur()))
                {
                    sb.Append(Cur());
                    Adv();
                }
                if (_pos < _src.Length && Cur() == '.' && _pos + 1 < _src.Length && char.IsDigit(_src[_pos + 1]))
                {
                    sb.Append(Cur());
                    Adv();
                    while (_pos < _src.Length && char.IsDigit(Cur()))
                    {
                        sb.Append(Cur());
                        Adv();
                    }
                }
                return new Token(TipoToken.ConstanteNumerica, sb.ToString(), l, c);
            }

            if (char.IsLetter(ch) || ch == '_')
            {
                var sb = new StringBuilder();
                while (_pos < _src.Length && (char.IsLetterOrDigit(Cur()) || Cur() == '_'))
                {
                    sb.Append(Cur());
                    Adv();
                }
                string lex = sb.ToString();
                return new Token(Reservadas.Contains(lex) ? TipoToken.PalabraReservada : TipoToken.Identificador, lex, l, c);
            }

            foreach (var op in new[] { "==", "!=", "<=", ">=" })
                if (Match(op)) return Consume(op, TipoToken.OperadorRelacional, l, c);

            foreach (var op in new[] { "&&", "||" })
                if (Match(op)) return Consume(op, TipoToken.OperadorLogico, l, c);

            if (ch == '<' || ch == '>')
                return Consume(ch.ToString(), TipoToken.OperadorRelacional, l, c);
            if (ch == '!')
                return Consume("!", TipoToken.OperadorLogico, l, c);
            if (ch == '=')
                return Consume("=", TipoToken.OperadorAsignacion, l, c);
            if ("+-*/^".IndexOf(ch) >= 0)
                return Consume(ch.ToString(), TipoToken.OperadorAritmetico, l, c);
            if ("{},();".IndexOf(ch) >= 0)
                return Consume(ch.ToString(), TipoToken.SimboloEspecial, l, c);

            string err = ch.ToString();
            Adv();
            return new Token(TipoToken.ErrorLexico, err, l, c);
        }

        private char Cur() => _src[_pos];

        private void Adv()
        {
            if (_src[_pos] == '\n')
            {
                _linea++;
                _col = 1;
            }
            else
            {
                _col++;
            }
            _pos++;
        }

        private bool Match(string s) => _pos + s.Length <= _src.Length && _src.Substring(_pos, s.Length) == s;

        private Token Consume(string s, TipoToken t, int l, int c)
        {
            for (int i = 0; i < s.Length; i++)
                Adv();
            return new Token(t, s, l, c);
        }
    }

    public partial class MainWindow : Window
    {
        private static readonly Dictionary<TipoToken, Color> Colores = new Dictionary<TipoToken, Color>
        {
            { TipoToken.PalabraReservada,   Color.FromRgb( 83,  74, 183) },
            { TipoToken.Identificador,      Color.FromRgb( 15, 110,  86) },
            { TipoToken.ConstanteNumerica,  Color.FromRgb(186, 117,  23) },
            { TipoToken.Cadena,             Color.FromRgb(216,  90,  48) },
            { TipoToken.OperadorAritmetico, Color.FromRgb( 55, 138, 221) },
            { TipoToken.OperadorRelacional, Color.FromRgb( 99, 153,  34) },
            { TipoToken.OperadorLogico,     Color.FromRgb(212,  83, 126) },
            { TipoToken.OperadorAsignacion, Color.FromRgb(136, 135, 128) },
            { TipoToken.SimboloEspecial,    Color.FromRgb( 68,  68,  65) },
            { TipoToken.Blanco,             Color.FromRgb( 44,  44,  42) },
            { TipoToken.ErrorLexico,         Color.FromRgb(226,  75,  74) },
        };

        private static readonly Dictionary<TipoToken, string> Etiquetas = new Dictionary<TipoToken, string>
        {
            { TipoToken.PalabraReservada,   "Palabra reservada"  },
            { TipoToken.Identificador,      "Identificador"      },
            { TipoToken.ConstanteNumerica,  "Constante numérica" },
            { TipoToken.Cadena,             "Cadena"             },
            { TipoToken.OperadorAritmetico, "Op. aritmético"     },
            { TipoToken.OperadorRelacional, "Op. relacional"     },
            { TipoToken.OperadorLogico,     "Op. lógico"         },
            { TipoToken.OperadorAsignacion, "Op. asignación"     },
            { TipoToken.SimboloEspecial,    "Símbolo especial"   },
            { TipoToken.Blanco,             "Blanco"             },
            { TipoToken.ErrorLexico,        "Error léxico"       },
        };

        private static readonly string[] Ejemplos =
        {
            "if (x == 10) {\n  print(\"Hola mundo\");\n}",
            "public void calcular(a, b) {\n  const resultado = a + b * 2.5;\n  if (resultado >= 100 || b != 0) {\n    return resultado;\n  }\n}",
            "var x = 10;\nconst y = @precio * x;\nbool ok = true && #flag;\nprint(x + $);"
        };

        private List<TokenItem> _tokens = new List<TokenItem>();

        public MainWindow()
        {
            InitializeComponent();
            dgTokens.ItemsSource = _tokens;
        }

        private void BtnAnalizar_Click(object sender, RoutedEventArgs e)
        {
            Analizar();
        }

        private void BtnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            Limpiar();
        }

        private void BtnSimple_Click(object sender, RoutedEventArgs e)
        {
            CargarEjemplo(0);
        }

        private void BtnIntermedio_Click(object sender, RoutedEventArgs e)
        {
            CargarEjemplo(1);
        }

        private void BtnError_Click(object sender, RoutedEventArgs e)
        {
            CargarEjemplo(2);
        }

        private void CargarEjemplo(int i)
        {
            var doc = new FlowDocument();
            var paragraph = new Paragraph(new Run(Ejemplos[i]));
            doc.Blocks.Add(paragraph);
            rtbCodigo.Document = doc;
            Analizar();
        }

        private void Analizar()
        {
            var textRange = new TextRange(rtbCodigo.Document.ContentStart, rtbCodigo.Document.ContentEnd);
            string src = textRange.Text;

            if (string.IsNullOrWhiteSpace(src))
                return;

            var tokens = new Lexer(src).Analizar();
            var noBlk = tokens.FindAll(t => t.Tipo != TipoToken.Blanco);
            int errs = tokens.FindAll(t => t.Tipo == TipoToken.ErrorLexico).Count;

            txtTotal.Text = noBlk.Count.ToString();
            txtReservadas.Text = tokens.FindAll(t => t.Tipo == TipoToken.PalabraReservada).Count.ToString();
            txtIdentificadores.Text = tokens.FindAll(t => t.Tipo == TipoToken.Identificador).Count.ToString();
            txtErrores.Text = errs.ToString();

            rtbColoreado.Document.Blocks.Clear();
            var coloredParagraph = new Paragraph();
            foreach (var t in tokens)
            {
                var run = new Run(t.Lexema);
                run.Foreground = new SolidColorBrush(Colores[t.Tipo]);
                coloredParagraph.Inlines.Add(run);
            }
            rtbColoreado.Document.Blocks.Add(coloredParagraph);
            txtPlaceholder.Visibility = Visibility.Collapsed;

            _tokens.Clear();
            int idx = 1;
            foreach (var t in tokens)
            {
                if (t.Tipo == TipoToken.Blanco) continue;

                var item = new TokenItem
                {
                    Numero = idx,
                    Lexema = t.Lexema.Replace("\n", "↵").Replace("\t", "→"),
                    Tipo = Etiquetas[t.Tipo],
                    Linea = t.Linea,
                    Columna = t.Columna,
                    Color = new SolidColorBrush(Colores[t.Tipo]),
                    EsError = t.Tipo == TipoToken.ErrorLexico
                };
                _tokens.Add(item);
                idx++;
            }

            dgTokens.Items.Refresh();

            if (errs > 0)
            {
                txtStatus.Foreground = new SolidColorBrush(Color.FromRgb(226, 75, 74));
                txtStatus.Text = $"  ⚠  {errs} error(es) léxico(s) detectado(s) — los tokens en rojo no pertenecen al lenguaje";
            }
            else
            {
                txtStatus.Foreground = new SolidColorBrush(Color.FromRgb(15, 110, 86));
                txtStatus.Text = $"  ✓  Análisis completado — {noBlk.Count} tokens reconocidos sin errores";
            }
        }

        private void Limpiar()
        {
            rtbCodigo.Document.Blocks.Clear();
            rtbColoreado.Document.Blocks.Clear();
            txtPlaceholder.Visibility = Visibility.Visible;

            _tokens.Clear();
            dgTokens.Items.Refresh();

            txtTotal.Text = "—";
            txtReservadas.Text = "—";
            txtIdentificadores.Text = "—";
            txtErrores.Text = "—";

            txtStatus.Foreground = Brushes.White;
            txtStatus.Text = "  Listo — ingresa código y presiona Analizar";
        }
    }
}