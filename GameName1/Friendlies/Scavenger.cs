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
    public class Scavenger : Targetable
    {

        public bool Alive;
        public Vector2 Position;
        public float speed;
        public Texture2D idleTexture;
        public Texture2D deathTexture;
        public Texture2D scavengingTexture;
        public AnimatedSprite activeTexture;
        public AnimatedSprite reverseTexture;
        public Texture2D sentOutHudTexture;
        public Texture2D sentBackHudTexture;
        public Texture2D notSentHudTexture;
        public Vector2 hudPosition;
        public int action; //0 means unsent, 1 means sent out, 2 means sent back, 3 means actively scavenging
        public List<Loot> scavengedLoot;
        double whenScavengeBegan;
        int actionToReturnTo;
        Vector2 scavengerSpawn;
        Vector2 scavengerIdle;
        Loot lootToLoot;

        /* Magic Numbers*/
        float speedValue = .8f;
        int animationSpeed = 10;
        int numMapRows = 1;
        int numMapColumns = 4;
        int ladderDetectX = 122;
        double timeToScavenge = 3000;
        int lootBuffer = 5;

        public bool isHit(Vector2 crosshairPosition)
        {
            Vector2 truePosition = Vector2.Subtract(crosshairPosition, Position);
            if (Alive &&
                truePosition.X >= 0 &&
                truePosition.Y >= 0 &&
                truePosition.X <= Width &&
                truePosition.Y <= Height)
            {
                Alive = false;
                return true;
            }
            return false;
        }

        public int Width
        {
            get { return activeTexture.Texture.Width / activeTexture.Columns; }
        }

        public int Height
        {
            get { return activeTexture.Texture.Height / activeTexture.Rows; }
        }

        virtual public void Initialize(ContentManager content, Vector2 position, Vector2 spawnPosition, Vector2 HUDPosition)
        {
            idleTexture = content.Load<Texture2D>("Graphics\\TrooperIdle");
            deathTexture = content.Load<Texture2D>("Graphics\\TrooperDead");
            scavengingTexture = content.Load<Texture2D>("Graphics\\TrooperScavenging");
            activeTexture = new AnimatedSprite(content.Load<Texture2D>("Graphics\\Trooper"), numMapRows, numMapColumns, animationSpeed);
            reverseTexture = new AnimatedSprite(content.Load<Texture2D>("Graphics\\TrooperReverse"), numMapRows, numMapColumns, animationSpeed);
            sentOutHudTexture = content.Load<Texture2D>("Graphics\\SendOutIcon");
            sentBackHudTexture = content.Load<Texture2D>("Graphics\\SendBackIcon");
            notSentHudTexture = content.Load<Texture2D>("Graphics\\NotSentIcon");
            speed = speedValue;
            Position = position;
            scavengerSpawn = spawnPosition;
            scavengerIdle = position;
            hudPosition = HUDPosition;
            Alive = true;
            action = 0;
            scavengedLoot = new List<Loot>();
            whenScavengeBegan = 0;
        }

        public void Update(int command, GameTime gameTime, Wave wave)
        {
            if (Alive)
            {
                if (action == 0) //Idling
                {
                    //If command is to go, then put in scavenging spawn point
                    if (command == 1) 
                    {
                        Position = scavengerSpawn;
                        action = 1;
                    }
                }
                else if (action == 1) //Going out
                {
                    //If command is to come back, turn around
                    if (command == 0)
                    {
                        if (Position.X <= ladderDetectX)
                        {
                            Position = scavengerIdle;
                            action = 0;
                        }
                        else 
                        {
                            Position = Pather.Move(Position, true, speed);
                            reverseTexture.Update();
                            action = 2;
                        }
                    }
                    else //else scavenge
                    {
                        //If no enemies, just move forward
                        if (wave.enemiesOnScreen.Count == 0)
                        {
                            Position = Pather.Move(Position, false, speed);
                            activeTexture.Update();
                        }
                        //Look for nearest unlooted body
                        Enemy closestEnemy = null;
                        float closestDistance = float.MaxValue;
                        for (int i = 0; i < wave.enemiesOnScreen.Count; i++)
                        {
                            if (!wave.enemiesOnScreen[i].Alive && !wave.enemiesOnScreen[i].isLooted)
                            {
                                float enemyDistance = Vector2.Distance(Position, wave.enemiesOnScreen[i].Position);
                                if (enemyDistance < closestDistance)
                                {
                                    closestEnemy = wave.enemiesOnScreen[i];
                                    closestDistance = enemyDistance;
                                }
                            }
                        }
                        if (closestEnemy != null) 
                        {
                            //If it is close enough to scavenge, do so
                            float myR = Width + Position.X;
                            float myL = Position.X;
                            float enemyR = closestEnemy.Width + closestEnemy.Position.X;
                            float enemyL = closestEnemy.Position.X;
                            if ((myR >= enemyL && myR <= enemyR) || (myL >= enemyL && myL <= enemyR))
                            {
                                action = 3;
                                lootToLoot = closestEnemy.loot;
                                closestEnemy.isLooted = true;
                                whenScavengeBegan = gameTime.TotalGameTime.TotalMilliseconds;
                                actionToReturnTo = 1;
                            }
                            else //else move towards it
                            {
                                if (enemyL > myL) //Move right
                                {
                                    Position = Pather.Move(Position, false, speed);
                                    activeTexture.Update();
                                }
                                else //Move left 
                                {
                                    Position = Pather.Move(Position, true, speed);
                                    reverseTexture.Update();
                                }
                            }
                        }
                        else //Shouldn't happen
                        {
                            Position = Pather.Move(Position, false, speed);
                            activeTexture.Update();
                        }
                    }
                }
                else if (action == 2) //Coming back
                {
                    if (command == 1)
                    {
                        Position = Pather.Move(Position, false, speed);
                        activeTexture.Update();
                        action = 1;
                    }
                    else
                    {
                        if (Position.X <= ladderDetectX)
                        {
                            Position = scavengerIdle;
                            action = 0;
                            Game1.scavengerAddToSupply(this);
                        }
                        else
                        {
                            Position = Pather.Move(Position, true, speed);
                            reverseTexture.Update();
                        }
                    }
                }
                else //Scavenging from body
                {
                    if (whenScavengeBegan != 0 && gameTime.TotalGameTime.TotalMilliseconds - whenScavengeBegan > timeToScavenge)
                    {
                        action = actionToReturnTo;
                        scavengedLoot.Add(lootToLoot);
                    }
                    else
                    {
                        if (command == 0) actionToReturnTo = 2;
                        else if (command == 1) actionToReturnTo = 1;
                    }
                }
            }
        }

        public void addLootToSupply(SniperRifle sr, MachineGun mg, Player player)
        {
            for (int i = 0; i < scavengedLoot.Count; i++)
            {
                scavengedLoot[i].addLoot(sr, mg, player);
            }
            scavengedLoot.Clear();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Alive) 
            {
                if (action == 0) spriteBatch.Draw(idleTexture, Position, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                else if (action == 1) activeTexture.Draw(spriteBatch, Position, 1f);
                else if (action == 2) reverseTexture.Draw(spriteBatch, Position, 1f);
                else
                {
                    spriteBatch.Draw(scavengingTexture, Position, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    lootToLoot.Draw(spriteBatch, new Vector2(Position.X, Position.Y - lootToLoot.texture.Height - lootBuffer));
                }
            }
            else spriteBatch.Draw(deathTexture, Position, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            Texture2D textureToDraw = notSentHudTexture;
            if (action == 1) textureToDraw = sentOutHudTexture;
            else if (action == 2) textureToDraw = sentBackHudTexture;
            else if (action == 3)
            {
                if (actionToReturnTo == 1) textureToDraw = sentOutHudTexture;
                else if (actionToReturnTo == 2) textureToDraw = sentBackHudTexture;
            }
            spriteBatch.Draw(textureToDraw, hudPosition, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }
    }
}