using System.Collections.Generic;

namespace KerbalConstructionTime
{
    public class KCT_UpgradingBuilding : IKCTBuildItem
    {
        [Persistent]
        public int upgradeLevel, currentLevel, launchpadID = 0;
        [Persistent]
        public string id, commonName;
        [Persistent]
        public double progress = 0, BP = 0, cost = 0;
        [Persistent]
        public bool UpgradeProcessed = false, isLaunchpad = false;
        //public bool allowUpgrade = false;
        private KCT_KSC _KSC = null;

        public KCT_UpgradingBuilding()
        {

        }

        public KCT_UpgradingBuilding(string facilityID, int newLevel, int oldLevel, string name)
        {
            id = facilityID;
            upgradeLevel = newLevel;
            currentLevel = oldLevel;
            commonName = name;

            KCTDebug.Log(string.Format("Upgrade of {0} requested from {1} to {2}", name, oldLevel, newLevel));
        }

        public void Downgrade()
        {
            KCTDebug.Log("Downgrading " + commonName + " to level " + currentLevel);
            if (isLaunchpad)
            {
                KSC.LaunchPads[launchpadID].level = currentLevel;
                if (KCT_GameStates.activeKSCName != KSC.KSCName || KCT_GameStates.ActiveKSC.ActiveLaunchPadID != launchpadID)
                {
                    return;
                }
            }
            foreach (Upgradeables.UpgradeableFacility facility in GetFacilityReferences())
            {
                KCT_Events.allowedToUpgrade = true;
                facility.SetLevel(currentLevel);
            }
            //KCT_Events.allowedToUpgrade = false;
        }

        public void Upgrade()
        {
            KCTDebug.Log("Upgrading " + commonName + " to level " + upgradeLevel);
            if (isLaunchpad)
            {
                KSC.LaunchPads[launchpadID].level = upgradeLevel;
                KSC.LaunchPads[launchpadID].DestructionNode = new ConfigNode("DestructionState");
                if (KCT_GameStates.activeKSCName != KSC.KSCName || KCT_GameStates.ActiveKSC.ActiveLaunchPadID != launchpadID)
                {
                    UpgradeProcessed = true;
                    return;
                }
                KSC.LaunchPads[launchpadID].Upgrade(upgradeLevel);
            }
            KCT_Events.allowedToUpgrade = true;
            foreach (Upgradeables.UpgradeableFacility facility in GetFacilityReferences())
            {
                facility.SetLevel(upgradeLevel);
            }
            int newLvl = KCT_Utilities.BuildingUpgradeLevel(id);
            UpgradeProcessed = (newLvl == upgradeLevel);

            KCTDebug.Log($"Upgrade processed: {UpgradeProcessed} Current: {newLvl} Desired: {upgradeLevel}");

            //KCT_Events.allowedToUpgrade = false;
        }

        public List<Upgradeables.UpgradeableFacility> GetFacilityReferences()
        {
            return ScenarioUpgradeableFacilities.protoUpgradeables[id].facilityRefs;
        }

        public void SetBP(double cost)
        {
            BP = CalculateBP(cost);
        }

        public bool AlreadyInProgress()
        {
            return (KSC != null);
        }

        public KCT_KSC KSC
        {
            get
            {
                if (_KSC == null)
                {
                    if (!isLaunchpad)
                        _KSC = KCT_GameStates.KSCs.Find(ksc => ksc.KSCTech.Find(ub => ub.id == this.id) != null);
                    else
                        _KSC = KCT_GameStates.KSCs.Find(ksc => ksc.KSCTech.Find(ub => ub.id == this.id && ub.isLaunchpad && ub.launchpadID == this.launchpadID) != null);
                }
                return _KSC;
            }
        }

        public string GetItemName()
        {
            return commonName;
        }

        public double GetBuildRate()
        {
            double rateTotal = 0;
            if (KSC != null)
            {
                rateTotal = KCT_Utilities.GetBothBuildRateSum(KSC);
            }
            return rateTotal;
        }

        public double GetTimeLeft()
        {
            return (BP - progress) / ((IKCTBuildItem)this).GetBuildRate();
        }

        public bool IsComplete()
        {
            return progress >= BP;
        }

        public KCT_BuildListVessel.ListType GetListType()
        {
            return KCT_BuildListVessel.ListType.KSC;
        }

        public void IncrementProgress(double UTDiff)
        {
            if (!IsComplete()) AddProgress(GetBuildRate() * UTDiff);
            if (HighLogic.LoadedScene == GameScenes.SPACECENTER && (IsComplete() || !KCT_PresetManager.Instance.ActivePreset.generalSettings.KSCUpgradeTimes))
            {
                if (ScenarioUpgradeableFacilities.Instance != null && KCT_GameStates.erroredDuringOnLoad.OnLoadFinished)
                    Upgrade();
            }
        }

        public static double CalculateBP(double cost)
        {
            double bp = KCT_MathParsing.GetStandardFormulaValue("KSCUpgrade", new Dictionary<string, string>() { { "C", cost.ToString() }, { "O", KCT_PresetManager.Instance.ActivePreset.timeSettings.OverallMultiplier.ToString() } });
            if (bp <= 0) { bp = 1; }

            return bp;
        }

        public static double CalculateBuildTime(double cost, KCT_KSC KSC = null)
        {
            double bp = CalculateBP(cost);
            double rateTotal = KCT_Utilities.GetBothBuildRateSum(KSC ?? KCT_GameStates.ActiveKSC);

            return bp / rateTotal;
        }

        private void AddProgress(double amt)
        {
            progress += amt;
            if (progress > BP) progress = BP;
        }
    }
}
