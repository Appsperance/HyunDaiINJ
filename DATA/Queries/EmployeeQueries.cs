namespace HyunDaiINJ.DATA.Queries
{
    public static class EmployeeQueries
    {
        public const string GetManagers = @"
            SELECT year, gender, name, department, shift, title, join_date 
            FROM employee 
            WHERE title = '사원'";

        public const string GetEmployeesByDepartment = @"
            SELECT year, gender, name, department, shift, title, join_date 
            FROM employee 
            WHERE department = @Department";
    }
}
