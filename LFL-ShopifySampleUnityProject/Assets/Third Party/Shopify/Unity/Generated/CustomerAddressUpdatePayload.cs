namespace Shopify.Unity {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Shopify.Unity.SDK;

    /// <summary>
    /// Return type for `customerAddressUpdate` mutation.
    /// </summary>
    public class CustomerAddressUpdatePayload : AbstractResponse, ICloneable {
        /// <summary>
        /// <see ref="CustomerAddressUpdatePayload" /> Accepts deserialized json data.
        /// <see ref="CustomerAddressUpdatePayload" /> Will further parse passed in data.
        /// </summary>
        /// <param name="dataJSON">Deserialized JSON data for CustomerAddressUpdatePayload</param>
        public CustomerAddressUpdatePayload(Dictionary<string, object> dataJSON) {
            DataJSON = dataJSON;
            Data = new Dictionary<string,object>();

            foreach (string key in dataJSON.Keys) {
                string fieldName = key;
                Regex regexAlias = new Regex("^(.+)___.+$");
                Match match = regexAlias.Match(key);

                if (match.Success) {
                    fieldName = match.Groups[1].Value;
                }

                switch(fieldName) {
                    case "customerAddress":

                    if (dataJSON[key] == null) {
                        Data.Add(key, null);
                    } else {
                        Data.Add(
                            key,

                            new MailingAddress((Dictionary<string,object>) dataJSON[key])
                        );
                    }

                    break;

                    case "customerUserErrors":

                    Data.Add(
                        key,

                        CastUtils.CastList<List<CustomerUserError>>((IList) dataJSON[key])
                    );

                    break;

                    case "userErrors":

                    Data.Add(
                        key,

                        CastUtils.CastList<List<UserError>>((IList) dataJSON[key])
                    );

                    break;
                }
            }
        }

        /// <summary>
        /// The customer’s updated mailing address.
        /// </summary>
        public MailingAddress customerAddress() {
            return Get<MailingAddress>("customerAddress");
        }

        /// <summary>
        /// List of errors that occurred executing the mutation.
        /// </summary>
        public List<CustomerUserError> customerUserErrors() {
            return Get<List<CustomerUserError>>("customerUserErrors");
        }

        /// \deprecated Use `customerUserErrors` instead
        /// <summary>
        /// List of errors that occurred executing the mutation.
        /// </summary>
        public List<UserError> userErrors() {
            return Get<List<UserError>>("userErrors");
        }

        public object Clone() {
            return new CustomerAddressUpdatePayload(DataJSON);
        }

        private static List<Node> DataToNodeList(object data) {
            var objects = (List<object>)data;
            var nodes = new List<Node>();

            foreach (var obj in objects) {
                if (obj == null) continue;
                nodes.Add(UnknownNode.Create((Dictionary<string,object>) obj));
            }

            return nodes;
        }
    }
    }
