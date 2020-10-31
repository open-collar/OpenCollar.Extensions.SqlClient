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

using OpenCollar.Extensions.Environment;

namespace TestRig
{
    internal sealed class SampleEnvironmentMetadata : EnvironmentMetadata
    {
        public SampleEnvironmentMetadata(string resourceName)
        {
            // uk-dev-webapp-1
            ResourceName = resourceName;
            if(string.IsNullOrWhiteSpace(resourceName))
            {
                IsEmulated = true;
                return;
            }

            var parts = resourceName.Split('-');

            if((parts.Length >= 1) && !string.IsNullOrWhiteSpace(parts[0]))
                Location = parts[0];

            if((parts.Length >= 2) && !string.IsNullOrWhiteSpace(parts[1]))
                Environment = parts[1];

            if((parts.Length >= 3) && !string.IsNullOrWhiteSpace(parts[2]))
                ResourceType = parts[2];

            if((parts.Length >= 4) && !string.IsNullOrWhiteSpace(parts[3]))
                Instance = parts[3];

            IsEmulated = !string.IsNullOrWhiteSpace(Location) && !string.IsNullOrWhiteSpace(Environment) && !string.IsNullOrWhiteSpace(ResourceType);
        }
    }
}