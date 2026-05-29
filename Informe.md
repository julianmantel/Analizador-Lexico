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
| Sintaxis | Llaves `{}` para bloques, `;` como terminador |
| Comentarios | No soportados en esta versión |
| Identificadores | Letras, dígitos y guion bajo; no pueden empezar con dígito |
| Casesensitive | Sí (`if` ≠ `IF`) |

### alfabeto del lenguaje

El alfabeto de entrada está compuesto por los caracteres ASCII imprimibles, incluyendo:

- Letras `a-z`, `A-Z`
- Dígitos `0-9`
- Símbolos: `+ - * / ^ = < > ! & | ( ) { } ; " .`
- Espacio, tabulación (`\t`), salto de línea (`\n`), retorno de carro (`\r`)

---

## 2. Tabla de tokens

| # | Token | Descripción | Ejemplo |
|---|---|---|---|
| 1 | Palabra reservada | Palabras con significado propio en el lenguaje | `if`, `else`, `const`, `var`, `bool`, `true`, `false`, `print`, `void`, `return`, `public` |
| 2 | Identificador | Nombres definidos por el usuario | `x`, `resultado`, `_contador`, `calcular` |
| 3 | Constante numérica | Valores numéricos enteros o decimales | `10`, `3.14`, `0`, `100` |
| 4 | Cadena | Texto literal delimitado por comillas dobles | `"Hola mundo"`, `""` |
| 5 | Operador aritmético | Operaciones matemáticas | `+`, `-`, `*`, `/`, `^` |
| 6 | Operador relacional | Comparaciones entre valores | `==`, `!=`, `<`, `>`, `<=`, `>=` |
| 7 | Operador lógico | Operaciones booleanas | `&&`, `\|\|`, `!` |
| 8 | Operador asignación | Asignación de valores | `=` |
| 9 | Símbolo especial | Delimitadores y separadores | `{`, `}`, `(`, `)`, `;` |
| 10 | Blanco | Espacios en blanco (se omiten en la tabla) | espacio, `\t`, `\n`, `\r` |
| 11 | Error léxico | Caracteres no reconocidos por el lenguaje | `@`, `#`, `$` |

---

## 3. Especificación léxica (expresiones regulares)

Cada clase léxica se define formalmente mediante una expresión regular sobre el alfabeto del lenguaje.

### 3.1. Blanco

```
Blanco = [ \t\n\r]+
```

Representa uno o más caracteres de espacio en blanco. Se reconocen pero se omiten de la tabla de tokens.

### 3.2. Cadena

```
Cadena = " ( [^"\\] | \\. )* "
```

Una cadena comienza y termina con comilla doble (`"`). Puede contener cualquier carácter excepto comilla doble y barra invertida, a menos que estén escapados con `\`.

### 3.3. Constante numérica

```
ConstanteNumerica = [0-9]+ ( \. [0-9]+ )?
```

Una o más cifras decimales, con una parte decimal opcional separada por punto. Ejemplos: `10`, `3.14`, `0`.

### 3.4. Identificador

```
Identificador = [a-zA-Z_] [a-zA-Z0-9_]*
```

Comienza con una letra o guion bajo, seguido de cero o más letras, dígitos o guiones bajos.

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
SimboloEspecial = { | } | ( | ) | ;
```

Delimitadores de bloques, paréntesis de agrupación y terminador de sentencia.

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

**Atributos internos:**

| Atributo | Tipo | Descripción |
|---|---|---|
| `_src` | `string` | Código fuente completo |
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
1. Si el carácter actual es whitespace → token Blanco
2. Si el carácter actual es '"' → buscar cierre de cadena → token Cadena o Error
3. Si el carácter actual es dígito → leer número (con decimales opcionales) → token ConstanteNumerica
4. Si el carácter actual es letra o '_' → leer identificar → resolver si es reservada
5. Si coincide con operador relacional de 2 chars (==, !=, <=, >=) → token OperadorRelacional
6. Si coincide con operador lógico de 2 chars (&&, ||) → token OperadorLogico
7. Si es <, > → token OperadorRelacional
8. Si es ! → token OperadorLogico
9. Si es = → token OperadorAsignacion
10. Si es +, -, *, /, ^ → token OperadorAritmetico
11. Si es {, }, (, ), ; → token SimboloEspecial
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

### 4.5. Interfaz gráfica (WPF)

La aplicación WPF presenta:

- **Editor de código fuente** (RichTextBox) — entrada del usuario
- **Tabla de tokens** (DataGrid) — muestra los tokens reconocidos con columnas: #, Lexema, Tipo (con color), Línea, Columna
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

| # | Lexema | Tipo | Línea | Columna |
|---|---|---|---|---|
| 1 | `if` | Palabra reservada | 1 | 1 |
| 2 | `(` | Símbolo especial | 1 | 4 |
| 3 | `x` | Identificador | 1 | 5 |
| 4 | `==` | Op. relacional | 1 | 7 |
| 5 | `10` | Constante numérica | 1 | 10 |
| 6 | `)` | Símbolo especial | 1 | 12 |
| 7 | `{` | Símbolo especial | 1 | 14 |
| 8 | `print` | Palabra reservada | 2 | 3 |
| 9 | `(` | Símbolo especial | 2 | 8 |
| 10 | `"Hola mundo"` | Cadena | 2 | 9 |
| 11 | `)` | Símbolo especial | 2 | 21 |
| 12 | `;` | Símbolo especial | 2 | 22 |
| 13 | `}` | Símbolo especial | 3 | 1 |

**Resultado esperado:** 13 tokens, 0 errores, 2 palabras reservadas (`if`, `print`), 1 identificador (`x`).

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

| # | Lexema | Tipo | Línea | Columna |
|---|---|---|---|---|
| 1 | `public` | Palabra reservada | 1 | 1 |
| 2 | `void` | Palabra reservada | 1 | 8 |
| 3 | `calcular` | Identificador | 1 | 13 |
| 4 | `(` | Símbolo especial | 1 | 21 |
| 5 | `a` | Identificador | 1 | 22 |
| 6 | `,` | Símbolo especial | 1 | 23 |
| 7 | `b` | Identificador | 1 | 25 |
| 8 | `)` | Símbolo especial | 1 | 26 |
| 9 | `{` | Símbolo especial | 1 | 28 |
| 10 | `const` | Palabra reservada | 2 | 3 |
| 11 | `resultado` | Identificador | 2 | 9 |
| 12 | `=` | Op. asignación | 2 | 20 |
| 13 | `a` | Identificador | 2 | 22 |
| 14 | `+` | Op. aritmético | 2 | 24 |
| 15 | `b` | Identificador | 2 | 26 |
| 16 | `*` | Op. aritmético | 2 | 28 |
| 17 | `2.5` | Constante numérica | 2 | 30 |
| 18 | `;` | Símbolo especial | 2 | 33 |
| 19 | `if` | Palabra reservada | 3 | 3 |
| 20 | `(` | Símbolo especial | 3 | 6 |
| 21 | `resultado` | Identificador | 3 | 7 |
| 22 | `>=` | Op. relacional | 3 | 17 |
| 23 | `100` | Constante numérica | 3 | 20 |
| 24 | `\|\|` | Op. lógico | 3 | 24 |
| 25 | `b` | Identificador | 3 | 27 |
| 26 | `!=` | Op. relacional | 3 | 29 |
| 27 | `0` | Constante numérica | 3 | 32 |
| 28 | `)` | Símbolo especial | 3 | 33 |
| 29 | `{` | Símbolo especial | 3 | 35 |
| 30 | `return` | Palabra reservada | 4 | 5 |
| 31 | `resultado` | Identificador | 4 | 12 |
| 32 | `;` | Símbolo especial | 4 | 21 |
| 33 | `}` | Símbolo especial | 5 | 3 |
| 34 | `}` | Símbolo especial | 6 | 1 |

**Resultado esperado:** 34 tokens, 0 errores, 5 palabras reservadas (`public`, `void`, `const`, `if`, `return`), 8 identificadores.

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

| # | Lexema | Tipo | Línea | Columna |
|---|---|---|---|---|
| 1 | `var` | Palabra reservada | 1 | 1 |
| 2 | `x` | Identificador | 1 | 5 |
| 3 | `=` | Op. asignación | 1 | 7 |
| 4 | `10` | Constante numérica | 1 | 9 |
| 5 | `;` | Símbolo especial | 1 | 11 |
| 6 | `const` | Palabra reservada | 2 | 1 |
| 7 | `y` | Identificador | 2 | 7 |
| 8 | `=` | Op. asignación | 2 | 9 |
| 9 | `@` | **Error léxico** | 2 | 11 |
| 10 | `precio` | Identificador | 2 | 12 |
| 11 | `*` | Op. aritmético | 2 | 19 |
| 12 | `x` | Identificador | 2 | 21 |
| 13 | `;` | Símbolo especial | 2 | 22 |
| 14 | `bool` | Palabra reservada | 3 | 1 |
| 15 | `ok` | Identificador | 3 | 6 |
| 16 | `=` | Op. asignación | 3 | 9 |
| 17 | `true` | Palabra reservada | 3 | 11 |
| 18 | `&&` | Op. lógico | 3 | 16 |
| 19 | `#` | **Error léxico** | 3 | 19 |
| 20 | `flag` | Identificador | 3 | 20 |
| 21 | `;` | Símbolo especial | 3 | 24 |
| 22 | `print` | Palabra reservada | 4 | 1 |
| 23 | `(` | Símbolo especial | 4 | 6 |
| 24 | `x` | Identificador | 4 | 7 |
| 25 | `+` | Op. aritmético | 4 | 9 |
| 26 | `$` | **Error léxico** | 4 | 11 |
| 27 | `)` | Símbolo especial | 4 | 12 |
| 28 | `;` | Símbolo especial | 4 | 13 |

**Resultado esperado:** 28 tokens, **3 errores léxicos** (`@`, `#`, `$`), 4 palabras reservadas (`var`, `const`, `bool`, `true`), 6 identificadores.

---

## 6. Conclusiones

El analizador léxico implementado demuestra que es posible construir un scanner funcional sin herramientas de generación automática, utilizando únicamente un enfoque de inspección secuencial de caracteres. Las expresiones regulares formalizadas en la sección 3 corresponden directamente a la lógica de decisión implementada en el método `Siguiente()` de la clase `Lexer`.

El diseño separa claramente las responsabilidades: el `Lexer` se encarga exclusivamente del análisis léxico, mientras que la capa de presentación (WPF) maneja la interacción con el usuario y la visualización de resultados.
