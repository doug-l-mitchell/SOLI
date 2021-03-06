using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using AutoFixture.Kernel;

namespace SoliTests.Repositories
{

    public class DbConnectionArg : ISpecimenBuilder
    {
        private readonly IDbConnection dbConnection;

        public DbConnectionArg(IDbConnection dbConnection)
        {
            this.dbConnection = dbConnection;
        }

        public object Create(object request, ISpecimenContext context)
        {
            var pi = request as ParameterInfo;
            if (pi == null)
            {
                return new NoSpecimen();
            }

            if (pi.ParameterType == typeof(IDbConnection))
            {
                return dbConnection;
            }

            return new NoSpecimen();
        }
    }
}
