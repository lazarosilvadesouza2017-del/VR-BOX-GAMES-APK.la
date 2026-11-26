using UnityEngine;
namespace SpiderVR
{
    public class SuitInfo { public string id, name, tex; public SuitInfo(string i, string n, string t) { id = i; name = n; tex = t; } }

    /// <summary>Trajes: modelos em Resources/Suits/id/model.*. Todos tem esqueleto, menos o Brand New Day (estatico).</summary>
    public static class SuitCatalog
    {
        public static readonly SuitInfo[] All = {
            new SuitInfo("agent_anti_venom", "Agent Anti-Venom", "Agent_Anti_Venom_D"),
            new SuitInfo("agent_venom", "Agent Venom", "Spiderman_AgentV_D"),
            new SuitInfo("all_new_spider_man", "All-New Spider-Man", "spiderman_newdesign_D"),
            new SuitInfo("ancient_venom", "Ancient Venom", "AncientVenom_D"),
            new SuitInfo("anti_venom", "Anti-Venom", "Anti_venom_D"),
            new SuitInfo("black_suit_spider_man_2018", "Traje Preto (2018)", "Symbiote_2018_D"),
            new SuitInfo("carnage", "Carnage", "Carnage_D"),
            new SuitInfo("classic_superior_spider_man", "Superior Spider-Man", "SpidermanSuperiorB_D"),
            new SuitInfo("iron_spider", "Iron Spider", "SpidermanIron_D"),
            new SuitInfo("iron_spider_battle_mode_infinity_war", "Iron Spider (Batalha)", "IronSpiderNewWithTentackles_D"),
            new SuitInfo("spider_man", "Amazing Spider-Man 2", "SONY_TASM2_chara_spiderman_COL_v03"),
            new SuitInfo("spider_man_homecoming", "Spider-Man Homecoming", "SpiderManHomecoming_D"),
            new SuitInfo("spider_venom", "Spider-Venom", "SpiderVenom_D"),
            new SuitInfo("brand_new_day", "Brand New Day", ""),
        };
        public static SuitInfo Get(string id) { foreach (var s in All) if (s.id == id) return s; return null; }

        /// <summary>Instancia o traje: altura 1,8 m, pes no chao do pai, textura aplicada, shader leve.</summary>
        public static GameObject Create(string id, Transform parent)
        {
            var info = Get(id); if (info == null) return null;
            var prefab = Resources.Load<GameObject>("Suits/" + info.id + "/model"); if (prefab == null) return null;
            var go = Object.Instantiate(prefab, parent, false); go.name = "Suit_" + id;
            var tex = string.IsNullOrEmpty(info.tex) ? null : Resources.Load<Texture2D>("Suits/" + info.id + "/" + info.tex);
            var light = Shader.Find("Mobile/Diffuse");
            foreach (var r in go.GetComponentsInChildren<Renderer>(true))
                foreach (var m in r.materials) { if (tex != null) { m.mainTexture = tex; if (light != null) m.shader = light; } }
            var b = Measure(go);
            if (b.size.y > 0.01f) go.transform.localScale *= 1.8f / b.size.y;
            b = Measure(go);
            go.transform.position += new Vector3(parent.position.x - b.center.x, parent.position.y - b.min.y, parent.position.z - b.center.z);
            return go;
        }
        static Bounds Measure(GameObject go)
        {
            var rs = go.GetComponentsInChildren<Renderer>(true); if (rs.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
            var b = rs[0].bounds; for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds); return b;
        }
    }
}
