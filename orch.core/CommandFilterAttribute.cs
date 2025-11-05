using System;
using System.Collections.Generic;

namespace orch.core
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class CommandFilterAttribute : Attribute
    {
        public CommandFilterAttribute()
        {
        }

        public CommandFilterAttribute(string commandTypeId)
        {
            if (!Guid.TryParse(commandTypeId, out var parsedId))
            {
                throw new ArgumentException($"Failed to parse '{commandTypeId}' as a GUID value.", nameof(commandTypeId));
            }

            CommandTypeId = parsedId;
        }

        public CommandFilterAttribute(Type commandDataType)
        {
            CommandDataType = commandDataType ?? throw new ArgumentNullException(nameof(commandDataType));
        }

        public Guid? CommandTypeId { get; }

        public string? CommandKey { get; set; }

        public Type? CommandDataType { get; }

        internal IEnumerable<Guid> ResolveCommandTypeIds()
        {
            if (CommandTypeId.HasValue)
            {
                yield return CommandTypeId.Value;
                yield break;
            }

            if (!string.IsNullOrWhiteSpace(CommandKey))
            {
                var infoByKey = OTransactionService.GetTypeIdByKey(CommandKey);
                if (infoByKey == null)
                {
                    throw new InvalidOperationException($"Command type with key '{CommandKey}' has not been registered.");
                }

                yield return infoByKey.TypeId;
                yield break;
            }

            if (CommandDataType != null)
            {
                var infoByType = OTransactionService.GetTypeIdByType(CommandDataType);
                if (infoByType == null)
                {
                    throw new InvalidOperationException($"Command data type '{CommandDataType.FullName}' has not been registered.");
                }

                yield return infoByType.TypeId;
                yield break;
            }

            throw new InvalidOperationException("CommandFilterAttribute must specify command type information.");
        }
    }
}
