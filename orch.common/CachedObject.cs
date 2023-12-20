using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace orch.common
{
    public delegate ObjectType RetrieveObjectDelegate<IDType, ObjectType>(IDType id);
    public delegate bool IsValueNullDelegate<T>(T v);
    public class CachedObject<IDType, ObjectType>
    {
        RetrieveObjectDelegate<IDType, ObjectType> rdel;
        public CachedObject(RetrieveObjectDelegate<IDType, ObjectType> retrieveDelegate)
        {
            rdel = retrieveDelegate;
        }
        Dictionary<IDType, ObjectType> _cache = new Dictionary<IDType, ObjectType>();
        public ObjectType this[IDType id]
        {
            get
            {
                ObjectType ret;
                if (_cache.TryGetValue(id, out ret))
                    return ret;
                ret = rdel(id);
                _cache.Add(id, ret);
                return ret;
            }
        }
    }
    

    public class ObjectDiffCache<IDType, ObjectType>
    {
        Dictionary<IDType, ObjectType> _cache = new Dictionary<IDType, ObjectType>();
        HashSet<IDType> _deletedID = new HashSet<IDType>();
        HashSet<IDType> _changedID = new HashSet<IDType>();
        HashSet<IDType> _newID = new HashSet<IDType>();
        List<ObjectType> _newNoID = new List<ObjectType>();

        RetrieveObjectDelegate<IDType, ObjectType> rdel;
        IsValueNullDelegate<IDType> isIDNull;
        IsValueNullDelegate<ObjectType> isObjectNull;
        public ObjectDiffCache(RetrieveObjectDelegate<IDType, ObjectType> retrieveDelegate
            , IsValueNullDelegate<IDType> isIDNull
            , IsValueNullDelegate<ObjectType> isObjectNull
            )
        {
            rdel = retrieveDelegate;
            this.isIDNull = isIDNull;
            this.isObjectNull = isObjectNull;
        }
        public void addObject(IDType id, ObjectType obj)
        {
            if (!isObjectNull(this[id]))
                throw new InvalidOperationException("Object already exists");
            _cache.Add(id, obj);
            _newID.Add(id);
        }
        public void addObject(ObjectType obj)
        {
            _newNoID.Add(obj);
        }
        public void deleteObject(IDType id)
        {
            if (_deletedID.Contains(id))
                return;
            if (_cache.ContainsKey(id))
            {
                _cache.Remove(id);
                if (_newID.Contains(id))
                {
                    _newID.Remove(id);
                    return;
                }
                if (_changedID.Contains(id))
                    _changedID.Add(id);
            }
            _deletedID.Add(id);
        }
        public ObjectType this[IDType id]
        {
            get
            {
                ObjectType ret;
                if (_cache.TryGetValue(id, out ret))
                    return ret;
                ret = rdel(id);
                _cache.Add(id, ret);
                return ret;
            }
            set
            {
                ObjectType ret;
                if (_cache.TryGetValue(id, out ret))
                {
                    _cache[id] = value;
                    return;
                }
                ret = rdel(id);
                if (isObjectNull(ret))
                    addObject(id, value);
                else
                    _changedID.Add(id);
            }
        }
        public List<IDType> getDeletedList()
        {
            return _deletedID.ToList();
        }
        public List<Tuple<IDType, ObjectType>> getUpdateList()
        {
            List<Tuple<IDType, ObjectType>> ret = new List<Tuple<IDType, ObjectType>>();
            foreach (IDType id in _changedID)
                ret.Add(new Tuple<IDType, ObjectType>(id, _cache[id]));
            return ret;
        }
        public List<Tuple<IDType, ObjectType>> getNewWithIDList()
        {
            List<Tuple<IDType, ObjectType>> ret = new List<Tuple<IDType, ObjectType>>();
            foreach (IDType id in _newID)
                ret.Add(new Tuple<IDType, ObjectType>(id, _cache[id]));
            return ret;
        }
        public List<ObjectType> getNewWithNoIDList()
        {
            return _newNoID.ToList();
        }
    }
}
