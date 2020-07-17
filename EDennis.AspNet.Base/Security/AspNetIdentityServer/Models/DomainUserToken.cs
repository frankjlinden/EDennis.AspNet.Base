﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;


namespace EDennis.AspNet.Base.Security {
    public class DomainUserToken : IdentityUserToken<Guid>, ITemporalEntity {

        public DomainUser User { get; set; }


        public SysStatus SysStatus { get; set; }
        public string SysUser { get; set; }
        public DateTime SysStart { get; set; }
        public DateTime SysEnd { get; set; }


        public void Patch(JsonElement jsonElement, ModelStateDictionary modelState, bool mergeCollections = true) {
            foreach (var prop in jsonElement.EnumerateObject()) {
                try {
                    switch (prop.Name) {
                        case "Value":
                        case "value":
                            Value = prop.Value.GetString();
                            break;
                        case "LoginProvider":
                        case "loginProvider":
                            LoginProvider = prop.Value.GetString();
                            break;
                        case "UserId":
                        case "userId":
                            UserId = prop.Value.GetGuid();
                            break;
                        case "Name":
                        case "name":
                            LoginProvider = prop.Value.GetString();
                            break;
                        case "Properties":
                        case "properties":
                            var properties = new Dictionary<string, string>();
                            prop.Value.EnumerateObject().ToList().ForEach(e => {
                                properties.Add(e.Name, e.Value.GetString());
                            });
                            if (mergeCollections && Properties != null)
                                foreach (var entry in properties)
                                    if (Properties.ContainsKey(entry.Key))
                                        Properties[entry.Key] = entry.Value;
                                    else
                                        Properties.Add(entry.Key, entry.Value);
                            else
                                Properties = properties;
                            break;
                        case "SysUser":
                        case "sysUser":
                            SysUser = prop.Value.GetString();
                            break;
                        case "SysStatus":
                        case "sysStatus":
                            SysStatus = (SysStatus)Enum.Parse(typeof(SysStatus),prop.Value.GetString());
                            break;
                        case "SysStart":
                        case "sysStart":
                            SysStart = prop.Value.GetDateTime();
                            break;
                        case "SysEnd":
                        case "sysEnd":
                            SysEnd = prop.Value.GetDateTime();
                            break;
                        default:
                            break;
                    }
                } catch (InvalidOperationException ex) {
                    modelState.AddModelError(prop.Name, $"{ex.Message}: Cannot parse value for {prop.Value} from {GetType().Name} JSON");
                }
            }
        }

        public void Update(object updated) {
            var obj = updated as DomainUserToken;
            Value = obj.Value;
            LoginProvider = obj.LoginProvider;
            UserId = obj.UserId;
            Name = obj.Name;
            SysUser = obj.SysUser;
            SysStatus = obj.SysStatus;
            SysStart = obj.SysStart;
            SysEnd = obj.SysEnd;
        }

    }
}
