﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;

namespace GameName1
{
    public class Wave2 : Wave
    {

        /* Magic Numbers */
        int wavesize = 10;
        double baseTime = 1000;
        double interval = 6500;

        override public void Initialize(ContentManager content, Vector2 position)
        {
            infiniteAmmoModeOn = false;
            infiniteFoodModeOn = false;
            waveSize = wavesize;
            spawnTimings = new List<double>(waveSize);
            enemiesToSpawn = new List<Enemy>(waveSize);
            lootList = new List<Loot>(waveSize);
            for (int i = 0; i < waveSize; i++)
            {
                enemiesToSpawn.Insert(i, new Enemy1());
                spawnTimings.Insert(i, baseTime + i * interval);
            }
            lootList.Add(new SniperAmmoLoot(2, content));
            lootList.Add(new FoodLoot(1, content));
            lootList.Add(new MachineGunAmmoLoot(10, content));
            lootList.Add(new FoodLoot(1, content));
            lootList.Add(new MachineGunAmmoLoot(4, content));
            lootList.Add(new SniperAmmoLoot(4, content));
            lootList.Add(new MachineGunAmmoLoot(6, content));
            lootList.Add(new FoodLoot(1, content));
            lootList.Add(new SniperAmmoLoot(3, content));
            lootList.Add(new SniperAmmoLoot(1, content));
            openingTextFilename = "Content//Text//Wave2Open.txt";
            base.Initialize(content, position);
        }

        override public void applyModes()
        {
            Game1.initializeAmmo();
            base.applyModes();
        }
    }
}