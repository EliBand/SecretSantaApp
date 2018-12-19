using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretSanta
{
    public enum SQLCommandType
    {
        Select,
        Distinct,
        Insert,
        Update,
        Delete,
    }
    public enum SQLConditionType
    {
        None,
        And,
        Or,
    }
    public enum SQLOperator
    {
        Equal,
        NEqual,
        Greater,
        Lesser,
        GEqual,
        LEqual,
        Between,
        Like,
        In,
    }
    public class SQLCommandBuilder
    {
        public SqlCommand Command { get; set; }
        private SQLCondition[] Conditions { get; set; } = null;
        private string[] Columns { get; set; } = null;
        private string[] Values { get; set; } = null;
        public SQLCommandBuilder(SqlConnection connection)
        {
            Command = new SqlCommand();
            Command.Connection = connection;
            try
            {
                Command.Connection.Open();
            }
            catch (Exception)
            {
                Console.WriteLine("Connection already open");
            }
        }
        public SQLCommandBuilder(SqlConnection connection, SQLCommandType type, string table, SQLCondition[] conditions = null, string[] columns = null, string[] values = null) :this(connection)
        {
            buildSQLStatement(type, table, conditions, columns, values);
        }
        private void buildSQLStatement(SQLCommandType type, string table)
        {
            using (Command)
            {
                string[] args = new string[5];

                string valueString = "";
                string columnString = "";
                if (Columns != null)
                {
                    int i = 0;
                    foreach (string c in Columns)
                    {
                        i++;
                        columnString += c;
                        valueString += "@" + c;
                        if (i < Columns.Length)
                        {
                            columnString += ", ";
                            valueString += ", ";
                        }
                    }
                }

                string conditionString = "";
                if (Conditions != null)
                {
                    int i = 0;
                    foreach (SQLCondition c in Conditions)
                    {
                        i++;
                        if (i > 1)
                        {
                            conditionString += " " + c.ConditionType + " ";
                        }
                        conditionString += c.Condition;
                    }
                }
                // Build statement arguments
                switch (type)
                {
                    case SQLCommandType.Select:
                        args[0] = "SELECT";
                        if (Columns == null)
                        {
                            columnString = "*";
                        }
                        args[1] = columnString;
                        args[2] = "FROM";
                        args[3] = table;
                        if (Conditions != null)
                        {
                            args[4] = "WHERE " + conditionString;
                        }
                        break;
                    case SQLCommandType.Distinct:
                        args[0] = "SELECT DISTINCT";
                        if (Columns == null)
                        {
                            columnString = "*";
                        }
                        args[1] = columnString;
                        args[2] = "FROM";
                        args[3] = table;
                        if (Conditions != null)
                        {
                            args[4] = "WHERE " + conditionString;
                        }
                        break;
                    case SQLCommandType.Insert:
                        args[0] = "INSERT INTO";
                        args[1] = table;
                        if (Columns != null)
                        {
                            args[2] = "(" + columnString + ")";
                        }
                        args[3] = "VALUES (" + valueString + ")";
                        int i = 0;
                        foreach (string v in Values)
                        {
                            Command.Parameters.AddWithValue("@" + Columns[i], v);
                            i++;
                        }
                        break;
                    case SQLCommandType.Update:
                        args[0] = "UPDATE";
                        args[1] = table;
                        args[2] = "SET";
                        if (Conditions != null)
                        {
                            args[3] = "WHERE " + conditionString;
                        }
                        break;
                    case SQLCommandType.Delete:
                        args[0] = "DELETE FROM";
                        args[1] = table;
                        if (Conditions != null)
                        {
                            args[2] = "WHERE " + conditionString;
                        }
                        break;
                    default:
                        break;
                }
                Command.CommandText = string.Join(" ", args);
            }
        }
        public void buildSQLStatement(SQLCommandType type, string table, SQLCondition[] conditions)
        {
            Conditions = conditions;
            buildSQLStatement(type, table);
        }
        public void buildSQLStatement(SQLCommandType type, string[] columns, string table, SQLCondition[] conditions)
        {
            Conditions = conditions;
            Columns = columns;
            buildSQLStatement(type, table);
        }
        public void buildSQLStatement(SQLCommandType type, string[] columns, string table, SQLCondition condition)
        {
            Conditions = new SQLCondition[1];
            Conditions[0] = condition;
            Columns = columns;
            buildSQLStatement(type, table);
        }
        public void buildSQLStatement(SQLCommandType type, string table, SQLCondition condition)
        {
            Conditions = new SQLCondition[1];
            Conditions[0] = condition;
            buildSQLStatement(type, table);
        }
        public void buildSQLStatement(SQLCommandType type, string table, string[] columns, string[] values)
        {
            Columns = columns;
            Values = values;
            buildSQLStatement(type, table);
        }
        public void buildSQLStatement(SQLCommandType type, string table, string column, string value)
        {
            Columns = new string[1];
            Columns[0] = column;
            Values = new string[1];
            Values[0] = value;
            buildSQLStatement(type, table);
        }
        public void buildSQLStatement(SQLCommandType type, string table, SQLCondition[] conditions = null, string[] columns = null, string[] values = null)
        {
            Conditions = conditions;
            Columns = columns;
            Values = values;
            buildSQLStatement(type, table);
        }
    }
    public class SQLCondition
    {
        public string Condition { get; set; } = "";
        public string ConditionType { get; set; } = "";
        private SQLCondition(string column, SQLOperator op, SQLConditionType conditionType = SQLConditionType.None)
        {
            string sqlOperator = "";
            switch (op)
            {
                case SQLOperator.Equal:
                    sqlOperator = "="; break;
                case SQLOperator.NEqual:
                    sqlOperator = "<>"; break;
                case SQLOperator.Greater:
                    sqlOperator = ">"; break;
                case SQLOperator.Lesser:
                    sqlOperator = "<"; break;
                case SQLOperator.GEqual:
                    sqlOperator = ">="; break;
                case SQLOperator.LEqual:
                    sqlOperator = "<="; break;
                case SQLOperator.Between:
                    sqlOperator = "BETWEEN"; break;
                case SQLOperator.Like:
                    sqlOperator = "LIKE"; break;
                case SQLOperator.In:
                    sqlOperator = "IN"; break;
                default:
                    break;
            }
            switch (conditionType)
            {
                case SQLConditionType.None:
                    ConditionType = ""; break;
                case SQLConditionType.And:
                    ConditionType = "AND"; break;
                case SQLConditionType.Or:
                    ConditionType = "OR"; break;
                default:
                    ConditionType = ""; break;
            }
            Condition = column + sqlOperator;
        }
        public SQLCondition(string column, SQLOperator op, string value) : this(column, op)
        {
            Condition += "'" + value + "'";
        }
        public SQLCondition(string column, SQLOperator op, string value, SQLConditionType conditionType = SQLConditionType.None) : this(column, op, conditionType)
        {
            Condition += "'" + value + "'";
        }
        public SQLCondition(string column, SQLOperator op, int value) : this(column, op)
        {
            Condition += value;
        }
        public SQLCondition(string column, SQLOperator op, int value, SQLConditionType conditionType = SQLConditionType.None) : this(column, op, conditionType)
        {
            Condition += value;
        }
    }
}
