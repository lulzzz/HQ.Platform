using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using HQ.Data.Contracts;
using Microsoft.AspNetCore.Http;

namespace HQ.Platform.Api.Models
{
    [DataContract]
    [KnownType(nameof(GetKnownTypes))]
    public struct EnvelopeBody
    {
        private static IEnumerable<Type> GetKnownTypes() => KnownTypesContext.GetKnownTypes();

        [DataMember]
        public object Data;

        [DataMember]
        public IList<Error> Errors;

        [DataMember]
        public bool HasErrors;

        [DataMember]
        public int Status;

        [DataMember]
        public IHeaderDictionary Headers;

        [DataMember]
        public PagingInfo Paging;
    }
}