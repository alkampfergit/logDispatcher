using System;
using System.Collections.Generic;

namespace Egress.Helpers
{
    public static class ComponentFactory
    {
        public static object Create(string componentFullName, Dictionary<string, object> parameters)
        {
            var type = Type.GetType(componentFullName, false);
            if (type == null)
            {
                throw new ArgumentException(nameof(componentFullName), $"Unable to find type {componentFullName}");
            }

            var caseInsensitiveParameters = new Dictionary<string, object>(parameters ?? new Dictionary<string, object>(), StringComparer.OrdinalIgnoreCase);

            var constructors = type.GetConstructors(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            foreach (var constructor in constructors)
            {
                List<Object> parameterList = new List<object>();
                var constructorParameters = constructor.GetParameters();
                foreach (var argument in constructorParameters)
                {
                    if (!caseInsensitiveParameters.ContainsKey(argument.Name))
                    {
                        //parameter is missing
                        continue;
                    }
                    parameterList.Add(caseInsensitiveParameters[argument.Name]);
                }

                return constructor.Invoke(parameterList.ToArray());
            }

            throw new ArgumentException(nameof(parameters), "No suitable constructor accepts those parameters");
        }
    }
}
