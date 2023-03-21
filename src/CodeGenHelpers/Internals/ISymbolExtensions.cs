using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeGenHelpers.Extensions;
using Microsoft.CodeAnalysis;

#pragma warning disable IDE0008
#pragma warning disable IDE0090
#pragma warning disable IDE1006
#nullable enable
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

        public static string GetGloballyQualifiedTypeName(this INamespaceOrTypeSymbol symbol)
        {
            var value = GetFullMetadataName(symbol);
            if(_fullNamesMaping.Any(x => x.Value == value))
            {
                return _fullNamesMaping.First(x => x.Value == value).Key;
            }
            else if(_fullNamesMaping.Any(x => x.Value == $"System.Nullable`1[{value}]"))
            {
                return _fullNamesMaping.First(x => x.Value == $"System.Nullable`1[{value}]").Key + "?";
            }

            return value;
        }

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
            return s is INamespaceSymbol symbol && symbol.IsGlobalNamespace;
        }

        public static string GetFullName(this INamespaceOrTypeSymbol type)
        {
            if (type is IArrayTypeSymbol arrayType)
            {
                return $"{arrayType.ElementType.GetFullName()}[]";
            }

            if (((ITypeSymbol)type).IsNullable(out var t) && t != null)
            {
                return $"System.Nullable`1[{t.GetFullName()}]";
            }

            var name = type.ToDisplayString() ?? string.Empty;

            if (!_fullNamesMaping.TryGetValue(name, out string output))
            {
                output = name;
            }

            return output;
        }
    }
}
