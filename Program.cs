using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;

namespace Task {

    class Program {
        static async Task<int> Main (string[] args) {
            var Modify_json = new CommandLineApplication () {
                Name = "modify_json",
                Description = "it should modify json data",
                ShortVersionGetter = () => "1.0.0"
            };

            Modify_json.Command ("modify", app => {
                app.Description = "change json data";
                var show = app.Option ("--show", "show data", CommandOptionType.SingleOrNoValue);
                var clear = app.Option ("--clear", "show data", CommandOptionType.SingleOrNoValue);
                var add = app.Option ("--add", "show data", CommandOptionType.MultipleValue);
                var update = app.Option ("--update", "show data", CommandOptionType.SingleOrNoValue);
                var delete = app.Option ("--delete", "show data", CommandOptionType.SingleOrNoValue);
                var done = app.Option ("--done", "show data", CommandOptionType.SingleOrNoValue);
                app.OnExecuteAsync (async cancellationToken => {
                    HttpClient client = new HttpClient ();
                    HttpRequestMessage req = new HttpRequestMessage (HttpMethod.Get, "http://localhost:3000/todo-list");
                    HttpResponseMessage resp = await client.SendAsync (req);
                    var jsondata = await resp.Content.ReadAsStringAsync ();
                    var data = JsonConvert.DeserializeObject<List<objek>> (jsondata);
                    if (show.HasValue ()) {
                        foreach (var item in data) {
                            Console.WriteLine (item.id + ". " + item.act);
                        }
                    }

                    if (clear.HasValue ()) {
                        var sure = Prompt.GetYesNo ("delete all data?", false);
                        foreach (var item in data) {
                            await client.DeleteAsync ("http://localhost:3000/todo-list/" + item.id);
                        }
                    }

                    if (delete.HasValue ()) {
                        var del = Convert.ToInt32 (delete.Value ());
                        foreach (var item in data) {
                            if (item.id == del) {
                                await client.DeleteAsync ("http://localhost:3000/todo-list/" + item.id);
                            }
                        }
                    }

                    if (update.HasValue ()) {
                        Console.WriteLine ("Enter your update : ");
                        var input = Console.ReadLine ();
                        objek obj = new objek () {
                            act = input
                        };
                        var toJson = JsonConvert.SerializeObject (obj);
                        var cnt = new StringContent (toJson, Encoding.UTF8, "application/json");
                        var id = Convert.ToInt32 (update.Value ());
                        foreach (var item in data) {
                            if (item.id == id) {
                                await client.PatchAsync ("http://localhost:3000/todo-list/" + item.id, cnt);
                            }
                        }
                    }

                    if (done.HasValue ()) {
                        var id = Convert.ToInt32 (done.Value ());
                        foreach (var item in data) {
                            if (item.id == id) {
                                objek obj = new objek () {
                                act = item.act,
                                status = true
                                };
                                var toJson = JsonConvert.SerializeObject (obj);
                                var cnt = new StringContent (toJson, Encoding.UTF8, "application/json");
                                await client.PatchAsync ("http://localhost:3000/todo-list/" + item.id, cnt);
                                Console.WriteLine("status number "+item.id+" done");
                            }
                        }
                    }

                    if (add.HasValue ()) {
                        var objek = new objek () {
                            id = Convert.ToInt32 (add.Values[0]),
                            act = add.Values[1]
                        };
                        var toJson = JsonConvert.SerializeObject (objek);
                        var cnt = new StringContent (toJson, Encoding.UTF8, "application/json");
                        var posts = await client.PostAsync ("http://localhost:3000/todo-list", cnt);
                    }
                });
            });

            Modify_json.OnExecute (() => {
                Modify_json.ShowHelp ();
            });
            return Modify_json.Execute (args);
        }
    }

    class objek {
        public int id { get; set; }
        public string act { get; set; }
        public bool status { get; set; }
    }
}