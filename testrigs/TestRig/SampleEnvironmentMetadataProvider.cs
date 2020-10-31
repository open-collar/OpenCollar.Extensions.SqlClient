/*
 * This file is part of OpenCollar.Extensions.SqlClient.
 *
 * OpenCollar.Extensions.SqlClient is free software: you can redistribute it
 * and/or modify it under the terms of the GNU General Public License as published
 * by the Free Software Foundation, either version 3 of the License, or (at your
 * option) any later version.
 *
 * OpenCollar.Extensions is distributed in the hope that it will be
 * useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public
 * License for more details.
 *
 * You should have received a copy of the GNU General Public License along with
 * OpenCollar.Extensions.  If not, see <https://www.gnu.org/licenses/>.
 *
 * Copyright © 2020 Jonathan Evans (jevans@open-collar.org.uk).
 */

using Microsoft.Extensions.Configuration;

using OpenCollar.Extensions.Environment;

namespace TestRig
{
    internal sealed class SampleEnvironmentMetadataProvider : EnvironmentMetadataProvider
    {
        private static readonly EnvironmentType[] _environmentTypes = new[]
        {
            new EnvironmentType("Development", "Development environment", 100, "D", "DV", "DEV"),
            new EnvironmentType("Quality Assurance", "Quality assurance environment", 500, "Q", "QA", "TEST"),
            new EnvironmentType("Integration Testing", "Integration testing environment", 700, "I", "IT", "INT", "IAT"),
            new EnvironmentType("User Acceptance Testing", "User acceptance testing environment", 800, "U", "UAT"),
            new EnvironmentType("Production", "Production environment", 1000, "P", "PDN", "PRD", "PROD")
        };

        public SampleEnvironmentMetadataProvider(IConfigurationRoot configuration) : base(configuration["Meta:Host"], _environmentTypes)
        {
        }

        public override IEnvironmentMetadata GetEnvironmentMetadata(string resourceName)
        {
            return new SampleEnvironmentMetadata(resourceName);
        }
    }
}