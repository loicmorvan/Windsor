// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Castle.Windsor.Core;

namespace Castle.Windsor.Windsor.Diagnostics;

public class DependencyDuplicate(
    DependencyModel dependency1,
    DependencyModel dependency2,
    DependencyDuplicationReason reason)
{
    public DependencyModel Dependency1 { get; } = dependency1;
    public DependencyModel Dependency2 { get; } = dependency2;
    public DependencyDuplicationReason Reason { get; } = reason;

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        return obj.GetType() == GetType() && Equals((DependencyDuplicate)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Dependency1.GetHashCode();
            hashCode = (hashCode * 397) ^ Dependency2.GetHashCode();
            hashCode = (hashCode * 397) ^ Reason.GetHashCode();
            return hashCode;
        }
    }

    private bool Equals(DependencyDuplicate other)
    {
        return Equals(Dependency1, other.Dependency1) && Equals(Dependency2, other.Dependency2) &&
               Reason.Equals(other.Reason);
    }
}