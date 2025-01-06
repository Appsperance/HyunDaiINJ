using System.Collections.Generic;
using Npgsql;
using HyunDaiINJ.DATA.DTO;
using WpfApp1;
using HyunDaiINJ.DATA.Queries;
using System;

namespace HyunDaiINJ.DATA.DAO
{
    public class EmployeeDAO : IEmployeeDAO
    {
        private readonly string _connectionString;

        public EmployeeDAO()
        {
            _connectionString = App.ConnectionString
                ?? throw new InvalidOperationException("ConnectionString has not been initialized.");
        }

        public List<EmployeeDTO> GetManagers()
        {
            var employees = new List<EmployeeDTO>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                string query = EmployeeQueries.GetManagers;

                using (var command = new NpgsqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            employees.Add(new EmployeeDTO
                            {
                                Year = reader["year"].ToString(),
                                Gender = reader["gender"].ToString(),
                                Name = reader["name"].ToString(),
                                Department = reader["department"].ToString(),
                                Shift = reader["shift"].ToString(),
                                Title = reader["title"].ToString(),
                                JoinDate = reader["join_date"].ToString()
                            });
                        }
                    }
                }
            }

            return employees;
        }

        public List<EmployeeDTO> GetEmployeesByDepartment(string department)
        {
            var employees = new List<EmployeeDTO>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                string query = EmployeeQueries.GetEmployeesByDepartment;

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Department", department);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            employees.Add(new EmployeeDTO
                            {
                                Year = reader["year"].ToString(),
                                Gender = reader["gender"].ToString(),
                                Name = reader["name"].ToString(),
                                Department = reader["department"].ToString(),
                                Shift = reader["shift"].ToString(),
                                Title = reader["title"].ToString(),
                                JoinDate = reader["join_date"].ToString()
                            });
                        }
                    }
                }
            }

            return employees;
        }
    }
}
