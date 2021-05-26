#if !DISABLE_CODEGENHELPERS_SYMBOLEXTENSIONS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace CodeGenHelpers.Internals
{
    internal static class ISymbolExtensions
    {
        private static readonly Dictionary<string, string> _fullNamesMaping = new Dictionary<string, string>
            (StringComparer.OrdinalIgnoreCase)
            {
                { "string",     typeof(string).ToString()  },
                { "long",       typeof(long).ToString()    },
                { "int",        typeof(int).ToString()     },
                { "short",      typeof(short).ToString()   },
                { "ulong",      typeof(ulong).ToString()   },
                { "uint",       typeof(uint).ToString()    },
                { "ushort",     typeof(ushort).ToString()  },
                { "byte",       typeof(byte).ToString()    },
                { "double",     typeof(double).ToString()  },
                { "float",      typeof(float).ToString()   },
                { "decimal",    typeof(decimal).ToString() },
                { "bool",       typeof(bool).ToString()    },
            };

        public static string GetFullMetadataName(this INamespaceOrTypeSymbol symbol)
        {
            ISymbol s = symbol;
            var sb = new StringBuilder(s.MetadataName);

            var last = s;
            s = s.ContainingSymbol;

            if (s == null)
            {
                return symbol.GetFullName();
            }

            while (!IsRootNamespace(s))
            {
                if (s is ITypeSymbol && last is ITypeSymbol)
                {
                    sb.Insert(0, '+');
                }
                else
                {
                    sb.Insert(0, '.');
                }
                sb.Insert(0, s.MetadataName);

                s = s.ContainingSymbol;
            }

            var namedType = symbol as INamedTypeSymbol;

            if (namedType?.TypeArguments.Any() ?? false)
            {
                var genericArgs = string.Join(",", namedType.TypeArguments.Select(GetFullMetadataName));
                sb.Append($"[{ genericArgs }]");
            }

            return sb.ToString();
        }

        private static bool IsRootNamespace(ISymbol s)
        {
            return s is INamespaceSymbol && ((INamespaceSymbol)s).IsGlobalNamespace;
        }

        public static string GetFullName(this INamespaceOrTypeSymbol type)
        {
            IArrayTypeSymbol arrayType = type as IArrayTypeSymbol;
            if (arrayType != null)
            {
                return $"{arrayType.ElementType.GetFullName()}[]";
            }

            ITypeSymbol t;
            if ((type as ITypeSymbol).IsNullable(out t))
            {
                return $"System.Nullable`1[{t.GetFullName()}]";
            }

            var name = type.ToDisplayString();

            string output;

            if (_fullNamesMaping.TryGetValue(name, out output))
            {
                output = name;
            }

            return output;
        }

        public static bool IsNullable(this ITypeSymbol type)
        {
            return ((type as INamedTypeSymbol)?.IsGenericType ?? false)
                && type.OriginalDefinition.ToDisplayString().Equals("System.Nullable<T>", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsNullable(this ITypeSymbol type, out ITypeSymbol nullableType)
        {
            if (type.IsNullable())
            {
                nullableType = ((INamedTypeSymbol)type).TypeArguments.First();
                return true;
            }
            else
            {
                nullableType = null;
                return false;
            }
        }
    }
}
#endif
