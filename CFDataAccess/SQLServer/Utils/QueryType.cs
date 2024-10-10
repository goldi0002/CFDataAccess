using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Text.RegularExpressions;

namespace CFDataAccess.SQLServer.Utils
{
    public static class QueryType
    {
        // Prefixes for various query types
        public const string SP = "SP";
        public const string USP = "USP";
        public const string FN = "FN";
        public const string TVF = "TVF";
        public const string VIEW = "V";
        public const string TR = "TR";
        public const string IFT = "IFT";
        public const string SVF = "SVF";
        public const string OTHER = "OTHER";
        public const string SELECT = "SEL";

        // Regular expression pattern to validate procedure names
        private const string ValidProcedureNamePattern = @"^[a-zA-Z0-9_]+$";

        /// <summary>
        /// Validates the procedure name against predefined rules.
        /// </summary>
        /// <param name="procedureName">The name of the procedure.</param>
        /// <returns>True if the name is valid; otherwise, false.</returns>
        public static bool IsValidProcedureName(string procedureName)
        {
            if (string.IsNullOrWhiteSpace(procedureName))
            {
                throw new ArgumentNullException(nameof(procedureName), "Procedure name cannot be null or empty.");
            }

            return Regex.IsMatch(procedureName, ValidProcedureNamePattern);
        }

        /// <summary>
        /// Checks if the given procedure name is a stored procedure based on its prefix.
        /// </summary>
        /// <param name="procedureName">The name of the stored procedure.</param>
        /// <returns>True if the name starts with a recognized prefix; otherwise, false.</returns>
        public static bool IsStoredProcedure(string procedureName)
        {
            if (!IsValidProcedureName(procedureName))
            {
                LogInvalidQueryName(procedureName);
                return false;
            }

            return procedureName.StartsWith(SP, StringComparison.OrdinalIgnoreCase) ||
                   procedureName.StartsWith(USP, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the query type based on the provided name.
        /// </summary>
        /// <param name="queryName">The name of the query (procedure, function, etc.).</param>
        /// <returns>The type of query.</returns>
        public static QueryTypeEnum GetQueryType(string queryName)
        {
            if(queryName.StartsWith(SELECT, StringComparison.OrdinalIgnoreCase))
            {
                return QueryTypeEnum.SelectQuery;
            }
            if (!IsValidProcedureName(queryName))
            {
                LogInvalidQueryName(queryName);
                return QueryTypeEnum.Other;
            }

            if (queryName.StartsWith(SP, StringComparison.OrdinalIgnoreCase))
            {
                return QueryTypeEnum.StandardStoredProcedure;
            }
            else if (queryName.StartsWith(USP, StringComparison.OrdinalIgnoreCase))
            {
                return QueryTypeEnum.UserDefinedStoredProcedure;
            }
            else if (queryName.StartsWith(FN, StringComparison.OrdinalIgnoreCase))
            {
                return QueryTypeEnum.Function;
            }
            else if (queryName.StartsWith(TVF, StringComparison.OrdinalIgnoreCase))
            {
                return QueryTypeEnum.TableValuedFunction;
            }
            else if (queryName.StartsWith(VIEW, StringComparison.OrdinalIgnoreCase))
            {
                return QueryTypeEnum.View;
            }
            else if (queryName.StartsWith(TR, StringComparison.OrdinalIgnoreCase))
            {
                return QueryTypeEnum.Trigger;
            }
            else if (queryName.StartsWith(IFT, StringComparison.OrdinalIgnoreCase))
            {
                return QueryTypeEnum.InlineTableValuedFunction;
            }
            else if (queryName.StartsWith(SVF, StringComparison.OrdinalIgnoreCase))
            {
                return QueryTypeEnum.ScalarValuedFunction;
            }
            return QueryTypeEnum.Other;
        }
        /// <summary>
        /// Logs invalid query names for audit and debugging purposes.
        /// </summary>
        /// <param name="queryName">The invalid query name.</param>
        private static void LogInvalidQueryName(string queryName)
        {
            throw new ArgumentException($"Invalid query name detected: {queryName}");
        }
    }

    public enum QueryTypeEnum
    {
        StandardStoredProcedure,
        UserDefinedStoredProcedure,
        Function,
        TableValuedFunction,
        View,
        Trigger,
        InlineTableValuedFunction,
        ScalarValuedFunction,
        Other,
        SelectQuery
    }
}
