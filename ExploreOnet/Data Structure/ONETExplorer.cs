using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace ExploreOnet
{
    public class ONETExplorer
    {
        bool assyncronousLoad = true;

        public List<Occupation> occupations;
        public List<Property> properties;

        public List<OccupationProperty> occupationProperties;
        public Dictionary<Occupation, List<OccupationProperty>> occupationPropertiesIdxOccupation;
        public Dictionary<Property, List<OccupationProperty>> occupationPropertiesIdxProperty;
        
        public Dictionary<Occupation, double> occupationPercentiles;

        public static ONETExplorer current;

        public Dictionary<Occupation, Dictionary<Occupation, double>> occupationMetric;
        public Dictionary<Property, Dictionary<Property, double>> propertyMetric;

        public ClusterNode<Occupation> occupationsCluster;
        public ClusterNode<Property> propertyCluster;

        public void addOccupationProperty(OccupationProperty opp)
        {
            occupationProperties.Add(opp);
            //if (!occupationPropertiesIdxOccupation.ContainsKey(opp.ocp))
            //    occupationPropertiesIdxOccupation.Add(opp.ocp, new List<OccupationProperty>());
            occupationPropertiesIdxOccupation[opp.ocp].Add(opp);

            if (!occupationPropertiesIdxProperty.ContainsKey(opp.property))
                occupationPropertiesIdxProperty.Add(opp.property, new List<OccupationProperty>());
            occupationPropertiesIdxProperty[opp.property].Add(opp);

        }

        public void removeOccupation(Occupation ocp)
        {
            occupations.Remove(ocp);
            occupationProperties.RemoveAll((x) => x.ocp == ocp);
            occupationPropertiesIdxOccupation.Remove(ocp);
            foreach(KeyValuePair<Property, List<OccupationProperty>> kkp in occupationPropertiesIdxProperty)
            {
                kkp.Value.RemoveAll((x) => x.ocp == ocp);
            }
        }

        public ONETExplorer()
        {
            current = this;
        }

        public List<Action> loadTasks;
        public void load()
        {
            //Initialize base structures
            occupations = new List<Occupation>();
            properties = new List<Property>();
            occupationProperties = new List<OccupationProperty>();
            occupationPropertiesIdxOccupation = new Dictionary<Occupation, List<OccupationProperty>>();
            occupationPropertiesIdxProperty = new Dictionary<Property, List<OccupationProperty>>();
            occupationPercentiles = new Dictionary<Occupation, double>();


            loadTasks = new List<Action>();

            //Populate loading tasks
            loadTasks.Add(readOccupations);
            loadTasks.Add(readAbilities);
            loadTasks.Add(readKnowledge);
            loadTasks.Add(readSkills);

            loadTasks.Add(cleanOccupations);
            loadTasks.Add(normalizeOccupationProperties);


            loadTasks.Add(computeOccupationPercentiles);

            loadTasks.Add(getOccupationMetric);
            loadTasks.Add(computePropertyMetric);

            loadTasks.Add(createOccupationCluster);
            loadTasks.Add(createPropertyCluster);


            //await Task.Run(new Action(readOccupations));

            //Init loading
            loadingStep();

            //Old method
            //readOccupations();
            //readAbilities();
            //readKnowledge();
            //readSkills();
            //cleanOccupations();
            //normalizeOccupationProperties();
            //computeMetric();
            //createOccupationCluster();
        }

        int breathTime = 300;
        private void loadingStep()
        {
            if (loadTasks.Count > 0)
            {
                if (assyncronousLoad)
                {
                    Task tt = new Task(loadTasks[0]);
                    tt.ContinueWith(taskOver);

                    tt.Start();
                }
                else
                {
                    Console.WriteLine("--- Beggining task: " + loadTasks[0].Method.Name + " ---");
                    loadTasks[0]();
                    Console.WriteLine("--- Task over: " + loadTasks[0].Method.Name + " ---");
                    loadTasks.RemoveAt(0);

                    //Give some time for the UI thread not to die before going to the next task
                    Timer timer = new Timer(breathTime);
                    timer.AutoReset = false;
                    timer.Enabled = true;
                    timer.Elapsed += BreathOver;
                    timer.Start();
                }
            }
            else
            {
                Console.WriteLine("----- LOADING DONE -----");
            }
        }

        private void BreathOver(object sender, ElapsedEventArgs e)
        {
            MainWindow.current.Dispatcher.Invoke(loadingStep);
        }
        public void taskOver(Task tt)
        {
            loadTasks.RemoveAt(0);
            MainWindow.current.Dispatcher.Invoke(loadingStep);
        }

        private void readOccupations()
        {

            using (StreamReader file = new StreamReader("RawData\\Occupation Data.txt"))
            {
                int counter = 0;
                string ln;

                while ((ln = file.ReadLine()) != null)
                {
                    counter++;

                    if (counter == 1)
                    {
                        //Ignore first line
                    }
                    else
                    {
                        //Parse occupation line
                        string[] lineData = ln.Split('\t');

                        if (lineData.Length != 3)
                            throw new Exception("Input mismatch on occupation data");

                        Occupation ocp = new Occupation(lineData[0], lineData[1], lineData[2]);
                        occupations.Add(ocp);
                    }
                }
                file.Close();
                Console.WriteLine("Occupations file has " + counter + " lines.");
            }

            //Populate ocp properties dictionary
            for (int i = 0; i < occupations.Count; i++)
                occupationPropertiesIdxOccupation.Add(occupations[i], new List<OccupationProperty>());
        }
        private void readAbilities()
        {
            string type = "Ability";

            using (StreamReader file = new StreamReader("RawData\\Abilities.txt"))
            {
                int counter = 0;
                int badCounter = 0;
                string ln;

                while ((ln = file.ReadLine()) != null)
                {
                    counter++;

                    if (counter == 1)
                        continue; //Ignore first line


                    //Parse ability line
                    string[] lineData = ln.Split('\t');


                    //Read occupation 0
                    string occupationCode = lineData[0];
                    Occupation ocp = occupations.Find((x) => x.code == occupationCode);

                    //Read ability 2
                    string abilityName = lineData[2];
                    Property abt = properties.Find((x) => x.name == abilityName && x.type == type);
                    //Add ability to list if it does not exist yet
                    if (abt == null)
                    {
                        abt = new Property(abilityName, type);
                        properties.Add(abt);
                    }


                    //Read Source and Value 3 and 4
                    double abilityValue;
                    string source = lineData[3];
                    //LV or IM sources
                    if (source == "LV")
                    {
                        abilityValue = double.Parse(lineData[4], CultureInfo.InvariantCulture) / 7;
                    }
                    else if (source == "IM")
                    {
                        abilityValue = double.Parse(lineData[4], CultureInfo.InvariantCulture) / 5;
                    }
                    else
                    {
                        badCounter++; //Bad line because soruce is unrecognizable
                        continue;
                    }

                    //Check if data source is better (prefer LV)
                    bool update = true;
                    OccupationProperty existing = occupationPropertiesIdxOccupation[ocp].Find((x) => x.property == abt);
                    if (source != "LV") //If source is not LV only add if it is non existant
                    {
                        if (existing == null) //If target does not exist
                            update = true;
                        else
                            update = false;
                    }
                    
                    //Update data
                    if (update)
                    {
                        if (existing == null)
                        {
                            //If it does not exist , add it
                            addOccupationProperty(new OccupationProperty(ocp, abt, abilityValue, source));
                        }
                        else
                        {
                            //If it exists, update it
                            existing.value = abilityValue;
                            existing.source = source;
                        }
                    }
                }
                file.Close();
                Console.WriteLine("Abilities file has " + counter + " lines.");
                Console.WriteLine("Abilities file has " + badCounter + " bad Lines.");
            }
        }
        private void readKnowledge()
        {
            string type = "Knowledge";

            using (StreamReader file = new StreamReader("RawData\\Knowledge.txt"))
            {
                int counter = 0;
                int badCounter = 0;
                string ln;

                while ((ln = file.ReadLine()) != null)
                {
                    counter++;

                    if (counter == 1)
                        continue; //Ignore first line


                    //Parse ability line
                    string[] lineData = ln.Split('\t');


                    //Read occupation 0
                    string occupationCode = lineData[0];
                    Occupation ocp = occupations.Find((x) => x.code == occupationCode);

                    //Read ability 2
                    string knowledgeName = lineData[2];
                    Property kno = properties.Find((x) => x.name == knowledgeName && x.type == type);
                    //Add ability to list if it does not exist yet
                    if (kno == null)
                    {
                        kno = new Property(knowledgeName, type);
                        properties.Add(kno);
                    }


                    //Read Source and Value 3 and 4
                    double knowledgeValue;
                    string source = lineData[3];
                    //LV or IM sources
                    if (source == "LV")
                    {
                        knowledgeValue = double.Parse(lineData[4], CultureInfo.InvariantCulture) / 7;
                    }
                    else if (source == "IM")
                    {
                        knowledgeValue = double.Parse(lineData[4], CultureInfo.InvariantCulture) / 5;
                    }
                    else
                    {
                        badCounter++; //Bad line because soruce is unrecognizable
                        continue;
                    }

                    //Check if data source is better (prefer LV)
                    bool update = true;
                    OccupationProperty existing = occupationPropertiesIdxOccupation[ocp].Find((x) => x.property == kno);
                    if (source != "LV") //If source is not LV only add if it is non existant
                    {
                        if (existing == null) //If target does not exist
                            update = true;
                        else
                            update = false;
                    }

                    //Update data
                    if (update)
                    {
                        if (existing == null)
                        {
                            //If it does not exist , add it
                            addOccupationProperty(new OccupationProperty(ocp, kno, knowledgeValue, source));
                        }
                        else
                        {
                            //If it exists, update it
                            existing.value = knowledgeValue;
                            existing.source = source;
                        }
                    }
                }
                file.Close();
                Console.WriteLine("Knowledges file has " + counter + " lines.");
                Console.WriteLine("Knowledges file has " + badCounter + " bad Lines.");
            }
        }
        private void readSkills()
        {
            string type = "Skill";

            using (StreamReader file = new StreamReader("RawData\\Skills.txt"))
            {
                int counter = 0;
                int badCounter = 0;
                string ln;

                while ((ln = file.ReadLine()) != null)
                {
                    counter++;

                    if (counter == 1)
                        continue; //Ignore first line


                    //Parse ability line
                    string[] lineData = ln.Split('\t');


                    //Read occupation 0
                    string occupationCode = lineData[0];
                    Occupation ocp = occupations.Find((x) => x.code == occupationCode);

                    //Read ability 2
                    string knowledgeName = lineData[2];
                    Property ski = properties.Find((x) => x.name == knowledgeName && x.type == type);
                    //Add ability to list if it does not exist yet
                    if (ski == null)
                    {
                        ski = new Property(knowledgeName, type);
                        properties.Add(ski);
                    }


                    //Read Source and Value 3 and 4
                    double knowledgeValue;
                    string source = lineData[3];
                    //LV or IM sources
                    if (source == "LV")
                    {
                        knowledgeValue = double.Parse(lineData[4], CultureInfo.InvariantCulture) / 7;
                    }
                    else if (source == "IM")
                    {
                        knowledgeValue = double.Parse(lineData[4], CultureInfo.InvariantCulture) / 5;
                    }
                    else
                    {
                        badCounter++; //Bad line because soruce is unrecognizable
                        continue;
                    }

                    //Check if data source is better (prefer LV)
                    bool update = true;
                    OccupationProperty existing = occupationPropertiesIdxOccupation[ocp].Find((x) => x.property == ski);
                    if (source != "LV") //If source is not LV only add if it is non existant
                    {
                        if (existing == null) //If target does not exist
                            update = true;
                        else
                            update = false;
                    }

                    //Update data
                    if (update)
                    {
                        if (existing == null)
                        {
                            //If it does not exist , add it
                            addOccupationProperty(new OccupationProperty(ocp, ski, knowledgeValue, source));
                        }
                        else
                        {
                            //If it exists, update it
                            existing.value = knowledgeValue;
                            existing.source = source;
                        }
                    }
                }
                file.Close();
                Console.WriteLine("Skills file has " + counter + " lines.");
                Console.WriteLine("Skills file has " + badCounter + " bad Lines.");
            }
        }

        private void cleanOccupations()
        {
            //Clear empty occupations
            List<Occupation> toRemove = new List<Occupation>();
            foreach (Occupation occupation in occupations)
            {
                Occupation ocp = occupation;
                if (occupationPropertiesIdxOccupation[ocp].Count == 0)
                    toRemove.Add(ocp);
            }
            int toRemoveCount = toRemove.Count;

            for(int i=0;i<toRemove.Count;i++)
            {
                removeOccupation(toRemove[i]);
            }

            Console.WriteLine("Removed " + toRemoveCount + " occupations for lack of data.");

        }
        private void normalizeOccupationProperties()
        {
            //This is not being used now

            Console.WriteLine("Normalizing occupation properties.");
            //Put all abilities in all ocupations
            int propertiesAdded = 0;
            foreach (Occupation occupation in occupations)
            {
                Occupation ocp = occupation;

                foreach (Property ppt in properties)
                {
                    if ((occupationPropertiesIdxOccupation[ocp].FirstOrDefault((x) => x.property == ppt) == null)) //If property doesn't exist for this occupation
                    {
                        addOccupationProperty(new OccupationProperty(ocp, ppt, 0, "NA")); //Add 0 value
                        propertiesAdded++;
                    }
                }
            }
            Console.WriteLine("Added " + propertiesAdded + " properties for lack of data.");
        }


        public void getOccupationMetric()
        {
            computeOccupationMetric();
            loadMetric();
        }
        [Conditional("DEBUG")]
        private void computeOccupationMetric()
        {
            Console.WriteLine("Computing occupation metric.");
            //Initialize dictionaries
            occupationMetric = new Dictionary<Occupation, Dictionary<Occupation, double>>();
            for (int i=0;i<occupations.Count;i++)
            {
                occupationMetric.Add(occupations[i], new Dictionary<Occupation, double>());
            }


            //Compute metrics
            double dist;
            int percent = 10;
            for (int i = 0; i < occupations.Count; i++)
            {
                Occupation ocp1 = occupations[i];
                for (int k = i; k < occupations.Count; k++)
                {
                    Occupation ocp2 = occupations[k];
                    dist = distanceOcp(ocp1, ocp2);
                    occupationMetric[ocp1][ocp2] = dist;
                    occupationMetric[ocp2][ocp1] = dist;
                }

                if((int)(100 * i/(float)occupations.Count) >= percent)
                {
                    Console.WriteLine(percent + "%");
                    percent += 10;
                }
            }
            Console.WriteLine("Metric computed.");

            saveMetric();
        }
        private double distanceOcp(Occupation ocp1, Occupation ocp2)
        {
            //if (occupationPropertiesIdxOccupation[ocp1].Count != occupationPropertiesIdxOccupation[ocp2].Count)
            //    throw new Exception("Invalid metric system");

            double db = 0;
            OccupationProperty p1;
            OccupationProperty p2;
            for (int i = 0; i < properties.Count; i++)
            {
                Property ppt = properties[i];
                p1 = occupationPropertiesIdxOccupation[ocp1].FirstOrDefault((x) => x.property == ppt);
                p2 = occupationPropertiesIdxOccupation[ocp2].FirstOrDefault((x) => x.property == ppt);

                if (p1 != null && p2 != null)
                {
                    db += Math.Pow(p1.value - p2.value, 2);
                }
                else
                    db += 0;
                
                //db += Math.Pow(occupationPropertiesIdxOccupation[ocp1].Find((x)=> x.property == ppt).value - occupationPropertiesIdxOccupation[ocp2].Find((x) => x.property == ppt).value, 2);
            }
            db = Math.Sqrt(db);

            return db;
        }
        public double distanceOcpCached(Occupation ocp1, Occupation ocp2)
        {
            return occupationMetric[ocp1][ocp2];
        }
        
        [Conditional("DEBUG")]
        public void saveMetric()
        {
            Console.WriteLine("Saving metric");

            DirectoryInfo dd = new DirectoryInfo(Directory.GetCurrentDirectory());
            dd = dd.Parent.Parent;
            dd = dd.EnumerateDirectories().First((x) => x.Name == "Resources");

            using (FileStream metricFile = new FileStream(dd.FullName + "\\occupationMetric.txt", FileMode.Create))
            {
                using (StreamWriter wr = new StreamWriter(metricFile))
                {
                    wr.Write("Nill");
                    for (int i = 0; i < occupations.Count; i++)
                    {
                        wr.Write("\t" + occupations[i].title);
                    }

                    for (int i = 0; i < occupations.Count; i++)
                    {
                        wr.Write("\n");
                        wr.Write(occupations[i].title);
                        for (int j = 0; j < occupations.Count; j++)
                        {
                            wr.Write("\t" + occupationMetric[occupations[i]][occupations[j]]);
                        }
                    }
                }
            }
        }

        [Conditional("RELEASE")]
        public void loadMetric()
        {
            Console.WriteLine("Loading metric.");
            string metrics = Properties.Resources.occupationMetric;

            occupationMetric = new Dictionary<Occupation, Dictionary<Occupation, double>>();
            for (int i = 0; i < occupations.Count; i++)
            {
                occupationMetric.Add(occupations[i], new Dictionary<Occupation, double>());
            }

            string[] lines = metrics.Split('\n');
            for (int i = 0; i < 0; i++)
            {
                if (lines[i + 1] != occupations[i].title)
                    throw new Exception("Invalid header on occupation metric file");
            }

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                string[] columns = line.Split('\t');

                if (columns.Length != occupations.Count + 1)
                    throw new Exception("Invalid column number on line " + i);

                Occupation ocp1 = occupations[i - 1];
                for (int j = 1; j < columns.Length; j++)
                {
                    Occupation ocp2 = occupations[j - 1];
                    occupationMetric[ocp1].Add(ocp2, double.Parse(columns[j]));
                }
            }
        }



        private void computePropertyMetric()
        {
            Console.WriteLine("Computing property metric.");
            //Initialize dictionaries
            propertyMetric = new Dictionary<Property, Dictionary<Property, double>>();
            for (int i = 0; i < properties.Count; i++)
            {
                propertyMetric.Add(properties[i], new Dictionary<Property, double>());
            }


            //Compute metrics
            double dist;
            int percent = 10;
            for (int i = 0; i < properties.Count; i++)
            {
                Property ppt1 = properties[i];
                for (int k = i; k < properties.Count; k++)
                {
                    Property ppt2 = properties[k];
                    dist = distancePpt(ppt1, ppt2);
                    propertyMetric[ppt1][ppt2] = dist;
                    propertyMetric[ppt2][ppt1] = dist;
                }

                if ((int)(100 * i / (float)properties.Count) >= percent)
                {
                    Console.WriteLine(percent + "%");
                    percent += 10;
                }
            }
            Console.WriteLine("Metric computed.");

        }

        private double distancePpt(Property ppt1, Property ppt2)
        {
            double db = 0;
            OccupationProperty p1;
            OccupationProperty p2;
            for (int i = 0; i < occupations.Count; i++)
            {
                Occupation ocp = occupations[i];
                p1 = occupationPropertiesIdxOccupation[ocp].FirstOrDefault((x) => x.property == ppt1);
                p2 = occupationPropertiesIdxOccupation[ocp].FirstOrDefault((x) => x.property == ppt2);

                if (p1 != null && p2 != null)
                {
                    db += Math.Pow(p1.value - p2.value, 2);
                }
                else
                    db += 0;

                //db += Math.Pow(occupationPropertiesIdxOccupation[ocp1].Find((x)=> x.property == ppt).value - occupationPropertiesIdxOccupation[ocp2].Find((x) => x.property == ppt).value, 2);
            }
            db = Math.Sqrt(db);

            return db;
        }

        private double distancePptCached(Property ppt1, Property ppt2)
        {
            return propertyMetric[ppt1][ppt2];
        }

        public void computeOccupationPercentiles()
        {
            List<(Occupation, double)> ocpPc = new List<(Occupation, double)>();
            foreach(Occupation ocp in occupations)
            {
                ocpPc.Add((ocp, occupationPropertiesIdxOccupation[ocp].Average((x) => x.value)));
            }
            ocpPc = ocpPc.OrderBy((x) => x.Item2).ToList();

            for(int i=0;i<ocpPc.Count;i++)
            {
                double percentile = i / (double)ocpPc.Count;
                occupationPercentiles.Add(ocpPc[i].Item1, percentile);
            }
        }

        public void createOccupationCluster()
        {
            Console.WriteLine("Creating occupation cluster");
            occupationsCluster = ClusterNode<Occupation>.createDendogram(occupations, distanceOcpCached);
        }

        public void createPropertyCluster()
        {
            Console.WriteLine("Creating Property cluster.");
            propertyCluster = ClusterNode<Property>.createDendogram(properties, distancePptCached);
        }
    }
}
