# Informe — Analizador Léxico

**Cátedra:** Teoría de Computabilidad 1
**Trabajo Práctico:** Analizador Léxico
**Herramienta:** C# / .NET 8 / WPF

---

## 1. Definición del lenguaje

Se define un lenguaje de programación imperativo de propósito general, orientado a la instrucción y sencillo de analizar. El lenguaje admite las siguientes construcciones sintácticas:

- **Declaración de variables:** `var x = valor;`
- **Constantes:** `const PI = 3.14;`
- **Tipos de datos básicos:** `bool` (valores `true` / `false`)
- **Estructuras de control:** `if` / `else`
- **Funciones:** `public void nombre(params) { ... }`
- **Salida por pantalla:** `print(expresion);`
- **Sentencia de retorno:** `return expresion;`

### Características del lenguaje

| Característica | Descripción |
|---|---|
| Tipado | Débilmente tipado (solo `bool` explícito) |
| Sintaxis | Llaves `{}` para bloques, `;` como terminador, `,` como separador de parámetros |
| Comentarios | No soportados en esta versión |
| Identificadores | Solo ASCII (`a-z`, `A-Z`, `0-9`, `_`); no pueden empezar con dígito |
| Casesensitive | Sí (`if` ≠ `IF`) |

### Alfabeto del lenguaje

El alfabeto de entrada está compuesto por caracteres ASCII, incluyendo:

- Letras `a-z`, `A-Z`
- Dígitos `0-9`
- Símbolos: `+ - * / ^ = < > ! & | ( ) { } ; , " . _`
- Espacio, tabulación (`\t`), salto de línea (`\n`)

---

## 2. Tabla de tokens

| # | Token | Descripción | Ejemplo |
|---|---|---|---|
| 1 | Palabra reservada | Palabras con significado propio en el lenguaje | `if`, `else`, `const`, `var`, `bool`, `true`, `false`, `print`, `void`, `return`, `public` |
| 2 | Identificador | Nombres definidos por el usuario (solo ASCII) | `x`, `miVar`, `contador`, `miFuncion` |
| 3 | Constante numérica | Valores numéricos enteros o decimales | `1`, `2.5`, `10.0` |
| 4 | Cadena | Texto literal delimitado por comillas dobles | `"Hola"`, `""` |
| 5 | Operador aritmético | Operaciones matemáticas | `+`, `-`, `*`, `/`, `^` |
| 6 | Operador relacional | Comparaciones entre valores | `==`, `!=`, `<`, `>`, `<=`, `>=` |
| 7 | Operador lógico | Condición booleana | `&&`, `\|\|`, `!` |
| 8 | Operador asignación | Asignación de valores | `=` |
| 9 | Símbolo especial | Delimitadores y separadores | `{`, `}`, `(`, `)`, `;`, `,` |
| 10 | Blanco | Espacios en blanco | espacio, `\t`, `\n` |
| 11 | Error léxico | Caracteres no reconocidos por el lenguaje | `@`, `#`, `$` |

---

## 3. Especificación léxica (expresiones regulares)

Cada clase léxica se define formalmente mediante una expresión regular sobre el alfabeto del lenguaje.

###3.1. Blanco

```
Blanco = [ \t\n]
```

Cada carácter de espacio en blanco se reconoce como un token Blanco individual. Se reconocen y se incluyen en la tabla de tokens.

### 3.2. Cadena

```
Cadena = " ( [^"\\] | \\. )* "
```

Una cadena comienza y termina con comilla doble (`"`). Puede contener cualquier carácter excepto comilla doble y barra invertida, a menos que estén escapados con `\`.

### 3.3. Constante numérica

```
ConstanteNumerica = [0-9]+ ( \. [0-9]+ )?
```

Una o más cifras decimales, con una parte decimal opcional separada por punto. Ejemplos: `1`, `2.5`, `10.0`.

### 3.4. Identificador

```
Identificador = [a-zA-Z_] [a-zA-Z0-9_]*
```

Comienza con una letra (`a-z`, `A-Z`) o guion bajo (`_`), seguido de cero o más letras, dígitos o guiones bajos. Solo se aceptan caracteres ASCII.

### 3.5. Palabra reservada

```
PalabraReservada = if | else | const | var | bool | true | false | print | void | return | public
```

Son identificadores con significado especial predefinido. Se resuelven por coincidencia exacta con la lista de palabras reservadas.

### 3.6. Operador relacional

```
OperadorRelacional = == | != | <= | >= | < | >
```

Incluye operadores de dos caracteres (`==`, `!=`, `<=`, `>=`) y de un carácter (`<`, `>`).

### 3.7. Operador lógico

```
OperadorLogico = && | \|\| | !
```

Incluye los operadores AND (`&&`), OR (`||`) y NOT (`!`).

### 3.8. Operador asignación

```
OperadorAsignacion = =
```

El signo de igual se reconoce como asignación. Para comparación de igualdad se usa `==`.

### 3.9. Operador aritmético

```
OperadorAritmetico = + | - | * | / | ^
```

Los cinco operadores aritméticos básicos, incluyendo potencia (`^`).

### 3.10. Símbolo especial

```
SimboloEspecial = { | } | ( | ) | ; | ,
```

Delimitadores de bloques, paréntesis de agrupación, terminador de sentencia y separador de parámetros.

### 3.11. Error léxico

```
ErrorLexico = . (cualquier carácter no reconocido)
```

Cualquier carácter que no pertenezca a ninguna de las clases anteriores se clasifica como error léxico.

---

## 4. Descripción del método de implementación

### 4.1. Arquitectura general

El analizador léxico se implementa como un **escáner de mano** (hand-written scanner) en C#, siguiendo un enfoque de **análisis descendente recursivo por caracteres**. No se utilizan herramientas generadoras de lexores (flex, ANTLR, etc.).

### 4.2. Clase Lexer

La clase `Lexer` encapsula toda la lógica de tokenización. Recibe el código fuente como un `string` y produce una lista de objetos `Token`.

**Preprocesamiento:** Antes del análisis, se eliminan los caracteres de retorno de carro (`\r`) del código fuente, ya que el `RichTextBox` de WPF utiliza `\r\n` para los saltos de línea.

**Atributos internos:**

| Atributo | Tipo | Descripción |
|---|---|---|
| `_src` | `string` | Código fuente completo (sin `\r`) |
| `_pos` | `int` | Posición actual de lectura en `_src` |
| `_linea` | `int` | Línea actual (inicia en 1) |
| `_col` | `int` | Columna actual (inicia en 1) |

**Métodos principales:**

| Método | Función |
|---|---|
| `Analizar()` | Punto de entrada. Recorre el código fuente y retorna una `List<Token>` |
| `Siguiente()` | Lee y retorna el siguiente token desde la posición actual |
| `Cur()` | Retorna el carácter en la posición actual sin avanzar |
| `Adv()` | Avanza una posición, actualizando línea y columna si es salto de línea |
| `Match(s)` | Verifica si la subcadena en `_pos` coincide con `s` |
| `Consume(s, tipo, l, c)` | Avanza `s.Length` posiciones y retorna un token del tipo indicado |

### 4.3. Algoritmo de análisis

El método `Siguiente()` implementa la siguiente lógica de decisión:

```
1. Si el carácter actual es whitespace → token Blanco (un carácter a la vez)
2. Si el carácter actual es '"' → buscar cierre de cadena → token Cadena o Error
3. Si el carácter actual es dígito (0-9) → leer número (con decimales opcionales) → token ConstanteNumerica
4. Si el carácter actual es letra ASCII (a-z, A-Z) o '_' → leer identificador → resolver si es reservada
5. Si coincide con operador relacional de 2 chars (==, !=, <=, >=) → token OperadorRelacional
6. Si coincide con operador lógico de 2 chars (&&, ||) → token OperadorLogico
7. Si es <, > → token OperadorRelacional
8. Si es ! → token OperadorLogico
9. Si es = → token OperadorAsignacion
10. Si es +, -, *, /, ^ → token OperadorAritmetico
11. Si es {, }, (, ), ;, , → token SimboloEspecial
12. Cualquier otro → token ErrorLexico
```

**Nota:** El orden de verificación es importante. Los operadores de dos caracteres se verifican antes que los de un carácter para evitar ambigüedades (por ejemplo, `==` debe reconocerse antes de `=`).

### 4.4. Modelo de datos

```csharp
public class Token
{
    public TipoToken Tipo { get; }    // Tipo de token
    public string Lexema { get; }     // Texto del token
    public int Linea { get; }         // Línea de aparición
    public int Columna { get; }       // Columna de aparición
}
```

###4.5. Interfaz gráfica (WPF)

La aplicación WPF presenta:

- **Editor de código fuente** (RichTextBox) — entrada del usuario, con interlineado corregido
- **Tabla de tokens** (DataGrid) — muestra todos los tokens incluyendo Blancos, con columnas: #, Lexema, Tipo (con color por tipo de token)
- **Estadísticas** — total de tokens, palabras reservadas, identificadores y errores
- **Barra de estado** — indica si hubo errores o el análisis fue exitoso

---

## 5. Casos de prueba

### Caso 1: Código simple

**Entrada:**
```
if (x == 10) {
  print("Hola mundo");
}
```

**Tokens esperados:**

| # | Lexema | Tipo |
|---|---|---|
| 1 | `if` | Palabra reservada |
| 2 | `(` | Símbolo especial |
| 3 | `x` | Identificador |
| 4 | ` ` | Blanco |
| 5 | `==` | Op. relacional |
| 6 | ` ` | Blanco |
| 7 | `10` | Constante numérica |
| 8 | `)` | Símbolo especial |
| 9 | ` ` | Blanco |
| 10 | `{` | Símbolo especial |
| 11 | `\n` | Blanco |
| 12 | `  ` | Blanco |
| 13 | `print` | Palabra reservada |
| 14 | `(` | Símbolo especial |
| 15 | `"Hola mundo"` | Cadena |
| 16 | `)` | Símbolo especial |
| 17 | `;` | Símbolo especial |
| 18 | `\n` | Blanco |
| 19 | `}` | Símbolo especial |

**Resultado esperado:** 19 tokens, 0 errores, 2 palabras reservadas (`if`, `print`), 1 identificador (`x`).

---

### Caso 2: Código intermedio

**Entrada:**
```
public void calcular(a, b) {
  const resultado = a + b * 2.5;
  if (resultado >= 100 || b != 0) {
    return resultado;
  }
}
```

**Tokens esperados:**

| # | Lexema | Tipo |
|---|---|---|
| 1 | `public` | Palabra reservada |
| 2 | ` ` | Blanco |
| 3 | `void` | Palabra reservada |
| 4 | ` ` | Blanco |
| 5 | `calcular` | Identificador |
| 6 | `(` | Símbolo especial |
| 7 | `a` | Identificador |
| 8 | `,` | Símbolo especial |
| 9 | ` ` | Blanco |
| 10 | `b` | Identificador |
| 11 | `)` | Símbolo especial |
| 12 | ` ` | Blanco |
| 13 | `{` | Símbolo especial |
| 14 | `\n` | Blanco |
| 15 | `  ` | Blanco |
| 16 | `const` | Palabra reservada |
| 17 | ` ` | Blanco |
| 18 | `resultado` | Identificador |
| 19 | ` ` | Blanco |
| 20 | `=` | Op. asignación |
| 21 | ` ` | Blanco |
| 22 | `a` | Identificador |
| 23 | ` ` | Blanco |
| 24 | `+` | Op. aritmético |
| 25 | ` ` | Blanco |
| 26 | `b` | Identificador |
| 27 | ` ` | Blanco |
| 28 | `*` | Op. aritmético |
| 29 | ` ` | Blanco |
| 30 | `2.5` | Constante numérica |
| 31 | `;` | Símbolo especial |
| 32 | `\n` | Blanco |
| 33 | `  ` | Blanco |
| 34 | `if` | Palabra reservada |
| 35 | `(` | Símbolo especial |
| 36 | `resultado` | Identificador |
| 37 | ` ` | Blanco |
| 38 | `>=` | Op. relacional |
| 39 | ` ` | Blanco |
| 40 | `100` | Constante numérica |
| 41 | ` ` | Blanco |
| 42 | `\|\|` | Op. lógico |
| 43 | ` ` | Blanco |
| 44 | `b` | Identificador |
| 45 | ` ` | Blanco |
| 46 | `!=` | Op. relacional |
| 47 | ` ` | Blanco |
| 48 | `0` | Constante numérica |
| 49 | `)` | Símbolo especial |
| 50 | ` ` | Blanco |
| 51 | `{` | Símbolo especial |
| 52 | ` ` | Blanco |
| 53 | `\n` | Blanco |
| 54 | `    ` | Blanco |
| 55 | `return` | Palabra reservada |
| 56 | ` ` | Blanco |
| 57 | `resultado` | Identificador |
| 58 | `;` | Símbolo especial |
| 59 | `\n` | Blanco |
| 60 | `  ` | Blanco |
| 61 | `}` | Símbolo especial |
| 62 | `\n` | Blanco |
| 63 | `}` | Símbolo especial |

**Resultado esperado:** 63 tokens, 0 errores, 5 palabras reservadas (`public`, `void`, `const`, `if`, `return`), 8 identificadores.

---

### Caso 3: Código con errores léxicos

**Entrada:**
```
var x = 10;
const y = @precio * x;
bool ok = true && #flag;
print(x + $);
```

**Tokens esperados:**

| # | Lexema | Tipo |
|---|---|---|
| 1 | `var` | Palabra reservada |
| 2 | ` ` | Blanco |
| 3 | `x` | Identificador |
| 4 | ` ` | Blanco |
| 5 | `=` | Op. asignación |
| 6 | ` ` | Blanco |
| 7 | `10` | Constante numérica |
| 8 | `;` | Símbolo especial |
| 9 | `\n` | Blanco |
| 10 | `const` | Palabra reservada |
| 11 | ` ` | Blanco |
| 12 | `y` | Identificador |
| 13 | ` ` | Blanco |
| 14 | `=` | Op. asignación |
| 15 | ` ` | Blanco |
| 16 | `@` | **Error léxico** |
| 17 | `precio` | Identificador |
| 18 | ` ` | Blanco |
| 19 | `*` | Op. aritmético |
| 20 | ` ` | Blanco |
| 21 | `x` | Identificador |
| 22 | `;` | Símbolo especial |
| 23 | `\n` | Blanco |
| 24 | `bool` | Palabra reservada |
| 25 | ` ` | Blanco |
| 26 | `ok` | Identificador |
| 27 | ` ` | Blanco |
| 28 | `=` | Op. asignación |
| 29 | ` ` | Blanco |
| 30 | `true` | Palabra reservada |
| 31 | ` ` | Blanco |
| 32 | `&&` | Op. lógico |
| 33 | ` ` | Blanco |
| 34 | `#` | **Error léxico** |
| 35 | `flag` | Identificador |
| 36 | `;` | Símbolo especial |
| 37 | `\n` | Blanco |
| 38 | `print` | Palabra reservada |
| 39 | `(` | Símbolo especial |
| 40 | `x` | Identificador |
| 41 | ` ` | Blanco |
| 42 | `+` | Op. aritmético |
| 43 | ` ` | Blanco |
| 44 | `$` | **Error léxico** |
| 45 | `)` | Símbolo especial |
| 46 | `;` | Símbolo especial |
| 47 | `\n` | Blanco |

**Resultado esperado:** 47 tokens, **3 errores léxicos** (`@`, `#`, `$`), 4 palabras reservadas (`var`, `const`, `bool`, `true`), 6 identificadores.

---

## 6. Conclusiones

El analizador léxico implementado demuestra que es posible construir un scanner funcional sin herramientas de generación automática, utilizando únicamente un enfoque de inspección secuencial de caracteres. Las expresiones regulares formalizadas en la sección 3 corresponden directamente a la lógica de decisión implementada en el método `Siguiente()` de la clase `Lexer`.

El diseño separa claramente las responsabilidades: el `Lexer` se encarga exclusivamente del análisis léxico, mientras que la capa de presentación (WPF) maneja la interacción con el usuario y la visualización de resultados. La tabla de tokens refleja fielmente el flujo de caracteres del código fuente, incluyendo los blancos como tokens individuales, lo que permite observar la estructura exacta del código tal como la interpreta el lexer.
