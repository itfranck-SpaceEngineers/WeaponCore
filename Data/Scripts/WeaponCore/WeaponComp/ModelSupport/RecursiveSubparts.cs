﻿using System.Collections;
using System.Collections.Generic;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace WeaponCore.Support
{
    // Courtesy Equinox
    /// <summary>
    /// Maintains a list of all recursive subparts of the given entity.  Respects changes to the model.
    /// </summary>
    public class RecursiveSubparts : IEnumerable<IMyEntity>
    {
        private readonly List<IMyEntity> _subparts = new List<IMyEntity>();
        private readonly Dictionary<string, IMyModelDummy> _tmp = new Dictionary<string, IMyModelDummy>();
        public readonly Dictionary<IMyEntity, string> EntityToName = new Dictionary<IMyEntity, string>();
        public readonly Dictionary<string, IMyEntity> NameToEntity = new Dictionary<string, IMyEntity>();

        private IMyModel _trackedModel;
        public IMyEntity Entity { get; set; }

        // not thread safe.
        public void CheckSubparts()
        {
            if (_trackedModel == Entity?.Model)
                return;
            _trackedModel = Entity?.Model;
            _subparts.Clear();
            EntityToName.Clear();
            NameToEntity.Clear();
            if (Entity != null)
            {
                var head = -1;
                _tmp.Clear();
                while (head < _subparts.Count)
                {
                    var query = head == -1 ? Entity : _subparts[head];
                    head++;
                    if (query.Model == null)
                        continue;
                    _tmp.Clear();
                    query.Model.GetDummies(_tmp);
                    foreach (var kv in _tmp)
                        if (kv.Key.StartsWith("subpart_"))
                        {
                            var name = kv.Key.Substring("subpart_".Length);
                            MyEntitySubpart res;
                            if (query.TryGetSubpart(name, out res))
                            {
                                _subparts.Add(res);
                                EntityToName.Add(res, name);
                                NameToEntity.Add(name, res);
                            }
                        }
                }
            }
        }
        IEnumerator<IMyEntity> IEnumerable<IMyEntity>.GetEnumerator()
        {
            return GetEnumerator();
        }

        public List<IMyEntity>.Enumerator GetEnumerator()
        {
            CheckSubparts();
            return _subparts.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Sets the emissive value of a specific emissive material on entity, and all recursive subparts.
        /// </summary>
        /// <param name="emissiveName">The name of the emissive material (ie. "Emissive0")</param>
        /// <param name="emissivity">Level of emissivity (0 is off, 1 is full brightness)</param>
        /// <param name="emissivePartColor">Color to emit</param>
        public void SetEmissiveParts(string emissiveName, Color emissivePartColor, float emissivity)
        {
            Entity.SetEmissiveParts(emissiveName, emissivePartColor, emissivity);
            SetEmissivePartsForSubparts(emissiveName, emissivePartColor, emissivity);
        }

        /// <summary>
        /// Sets the emissive value of a specific emissive material on all recursive subparts.
        /// </summary>
        /// <param name="emissiveName">The name of the emissive material (ie. "Emissive0")</param>
        /// <param name="emissivity">Level of emissivity (0 is off, 1 is full brightness).</param>
        /// <param name="emissivePartColor">Color to emit</param>
        public void SetEmissivePartsForSubparts(string emissiveName, Color emissivePartColor, float emissivity)
        {
            foreach (var k in this)
                k.SetEmissiveParts(emissiveName, emissivePartColor, emissivity);
        }
    }
}