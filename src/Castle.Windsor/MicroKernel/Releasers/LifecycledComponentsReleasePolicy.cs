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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using Castle.Core;
using Castle.Windsor.Core.Internal;
using Castle.Windsor.MicroKernel.Internal;
using Castle.Windsor.Windsor.Diagnostics;

namespace Castle.Windsor.MicroKernel.Releasers;

using Lock = Lock;

/// <summary>Tracks all components requiring decomission (<see cref = "Burden.RequiresPolicyRelease" />)</summary>
[Serializable]
public class LifecycledComponentsReleasePolicy : IReleasePolicy
{
	private readonly Dictionary<object, Burden> _instance2Burden = new(ReferenceEqualityComparer<object>.Instance);

	private readonly Lock _lock = Lock.Create();
	private readonly ITrackedComponentsPerformanceCounter _perfCounter;
	private ITrackedComponentsDiagnostic _trackedComponentsDiagnostic;

	/// <param name = "kernel">Used to obtain <see cref = "ITrackedComponentsDiagnostic" /> if present.</param>
	public LifecycledComponentsReleasePolicy(IKernel kernel)
		: this(GetTrackedComponentsDiagnostic(kernel), null)
	{
	}

	/// <summary>
	///     Creates new policy which publishes its tracking components count to
	///     <paramref
	///         name = "trackedComponentsPerformanceCounter" />
	///     and exposes diagnostics into
	///     <paramref
	///         name = "trackedComponentsDiagnostic" />
	///     .
	/// </summary>
	/// <param name = "trackedComponentsDiagnostic"></param>
	/// <param name = "trackedComponentsPerformanceCounter"></param>
	public LifecycledComponentsReleasePolicy(ITrackedComponentsDiagnostic trackedComponentsDiagnostic,
		ITrackedComponentsPerformanceCounter trackedComponentsPerformanceCounter)
	{
		_trackedComponentsDiagnostic = trackedComponentsDiagnostic;
		_perfCounter = trackedComponentsPerformanceCounter ?? NullPerformanceCounter.Instance;

		if (trackedComponentsDiagnostic != null) trackedComponentsDiagnostic.TrackedInstancesRequested += trackedComponentsDiagnostic_TrackedInstancesRequested;
	}

	private LifecycledComponentsReleasePolicy(LifecycledComponentsReleasePolicy parent)
		: this(parent._trackedComponentsDiagnostic, parent._perfCounter)
	{
	}

	private Burden[] TrackedObjects
	{
		get
		{
			using var holder = _lock.ForReading(false);
			if (holder.LockAcquired == false)
			{
				// TODO: that's sad... perhaps we should have waited...? But what do we do now? We're in the debugger. If some thread is keeping the lock
				// we could wait indefinatelly. I guess the best way to proceed is to add a 200ms timepout to accquire the lock, and if not succeeded
				// assume that the other thread just waits and is not going anywhere and go ahead and read this anyway...
			}

			var array = _instance2Burden.Values.ToArray();
			return array;
		}
	}

	public void Dispose()
	{
		KeyValuePair<object, Burden>[] burdens;
		using (_lock.ForWriting())
		{
			if (_trackedComponentsDiagnostic != null)
			{
				_trackedComponentsDiagnostic.TrackedInstancesRequested -=
					trackedComponentsDiagnostic_TrackedInstancesRequested;
				_trackedComponentsDiagnostic = null;
			}

			burdens = _instance2Burden.ToArray();
			_instance2Burden.Clear();
		}

		// NOTE: This is relying on a undocumented behavior that order of items when enumerating Dictionary<> will be oldest --> latest
		foreach (var burden in burdens.Reverse())
		{
			burden.Value.Released -= OnInstanceReleased;
			_perfCounter.DecrementTrackedInstancesCount();
			burden.Value.Release();
		}
	}

	public IReleasePolicy CreateSubPolicy()
	{
		var policy = new LifecycledComponentsReleasePolicy(this);
		return policy;
	}

	public bool HasTrack(object instance)
	{
		if (instance == null) return false;

		using (_lock.ForReading())
		{
			return _instance2Burden.ContainsKey(instance);
		}
	}

	public void Release(object instance)
	{
		if (instance == null) return;

		Burden burden;
		using (_lock.ForWriting())
		{
			// NOTE: we don't physically remove the instance from the instance2Burden collection here.
			// we do it in OnInstanceReleased event handler
			if (_instance2Burden.TryGetValue(instance, out burden) == false) return;
		}

		burden.Release();
	}

	public virtual void Track(object instance, Burden burden)
	{
		if (burden.RequiresPolicyRelease == false)
		{
			var lifestyle = (object)burden.Model.CustomLifestyle ?? burden.Model.LifestyleType;
			throw new ArgumentException(
				string.Format(
					"Release policy was asked to track object '{0}', but its burden has 'RequiresPolicyRelease' set to false. If object is to be tracked the flag must be true. This is likely a bug in the lifetime manager '{1}'.",
					instance, lifestyle));
		}

		try
		{
			using (_lock.ForWriting())
			{
				_instance2Burden.Add(instance, burden);
			}
		}
		catch (ArgumentNullException)
		{
			//eventually we should probably throw something more useful here too
			throw;
		}
		catch (ArgumentException)
		{
			throw HelpfulExceptionsUtil.TrackInstanceCalledMultipleTimes(instance, burden);
		}

		burden.Released += OnInstanceReleased;
		_perfCounter.IncrementTrackedInstancesCount();
	}

	private void OnInstanceReleased(Burden burden)
	{
		using (_lock.ForWriting())
		{
			if (_instance2Burden.Remove(burden.Instance) == false) return;
		}

		burden.Released -= OnInstanceReleased;
		_perfCounter.DecrementTrackedInstancesCount();
	}

	private void trackedComponentsDiagnostic_TrackedInstancesRequested(object sender, TrackedInstancesEventArgs e)
	{
		e.AddRange(TrackedObjects);
	}

	/// <summary>Obtains <see cref = "ITrackedComponentsDiagnostic" /> from given <see cref = "IKernel" /> if present.</summary>
	/// <param name = "kernel"></param>
	/// <returns></returns>
	public static ITrackedComponentsDiagnostic GetTrackedComponentsDiagnostic(IKernel kernel)
	{
		var diagnosticsHost = (IDiagnosticsHost)kernel.GetSubSystem(SubSystemConstants.DiagnosticsKey);
		return diagnosticsHost?.GetDiagnostic<ITrackedComponentsDiagnostic>();
	}

	/// <summary>Creates new <see cref = "ITrackedComponentsPerformanceCounter" /> from given <see cref = "IPerformanceMetricsFactory" />.</summary>
	/// <param name = "perfMetricsFactory"></param>
	/// <returns></returns>
	[SecuritySafeCritical]
	public static ITrackedComponentsPerformanceCounter GetTrackedComponentsPerformanceCounter(
		IPerformanceMetricsFactory perfMetricsFactory)
	{
		return NullPerformanceCounter.Instance;
	}
}