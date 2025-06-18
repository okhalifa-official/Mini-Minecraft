using Microsoft.VisualBasic.ApplicationServices;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace Minecraft
{
    public class MyGraphics
    {
        public int[] size;
        public int fps;
        public int scalar;

        public MyGraphics()
        {
            size = new int[2];
            size[0] = 2880;
            size[1] = 1800;
            scalar = size[0] / 20;
            fps = 30;
        }
    };

    public class WeatherEffect
    {
        public int curState;
        public Bitmap? skin;

        public WeatherEffect(int curWeather)
        {
            curState = curWeather;
            if (curState > 0)
            {
                skin = new Bitmap("..\\..\\..\\..\\..\\assets\\environment\\weather" + curState.ToString() + ".png");
            }
            else
            {
                skin = null;
            }
        }
    }
    public class Item : PhysicObject
    {
        public int id;
        public String name;
        public List<Item> required;
        public List<int> required_quantity;
        public Bitmap skin;
        public int dir;
        public List<String> states;
        public int curState;
        public int quantity;
        public int stack_max;
        public String placementBehavior;

        public Item()
        {
        }

        public Item(Item other)
        {
            this.id = other.id;
            this.name = other.name;
            this.required = other.required;
            this.required_quantity = other.required_quantity;
            this.dir = other.dir;
            this.states = other.states;
            this.curState = other.curState;
            this.skin = new Bitmap(states[curState]);
            this.quantity = other.quantity;
            this.stack_max = other.stack_max;
            this.placementBehavior = other.placementBehavior;
            for (int i = 0; i < other.states.Count; i++)
            {
                this.states.Add(other.states[i]);
            }
        }
    }

    public class Block : Item
    {
        public int health, max_health;
        public Block(Item item, int xx, int yy, int ww, int hh)
        {
            skin = new Bitmap(item.skin);
            this.x = xx;
            this.y = yy;
            this.w = ww;
            this.h = hh;
            this.health = 120;
            this.max_health = 120;
        }
    };

    public class Lava : Item
    {
        public List<Block> path;

        public Lava(Item item, int xx, int yy, int ww, int hh)
        {
            skin = new Bitmap(item.skin);
            path = new List<Block>();
            this.x = xx;
            this.y = yy;
            this.w = ww;
            this.h = hh;
            this.states = item.states;
            this.curState = item.curState;
        }
    }

    public class Ladder : Item
    {
        public Ladder(Item item, int xx, int yy, int ww, int hh)
        {
            skin = new Bitmap(item.skin);
            this.x = xx;
            this.y = yy;
            this.w = ww;
            this.h = hh;
            this.states = item.states;
            this.curState = item.curState;
        }
    }

    public class Tiles
    {
        public List<List<Block?>> grid;
        public int row;
        public int col;
        public int blockSize;

        public Tiles(MyGraphics myGraphics)
        {
            row = 110*(myGraphics.size[1] / myGraphics.scalar);
            col = 110*(21);
            blockSize = myGraphics.scalar;
            grid = new List<List<Block?>>(row+1);
            for(int i=0; i<row+1; i++)
            {
                List<Block?> rowList = new List<Block?>(col);
                for (int j = 0; j < col; j++)
                {
                    rowList.Add(null);
                }
                grid.Add(rowList);
            }
        }

        public void addBlock(int r, int c, Item item)
        {
            grid[r][c] = new Block(item, c*blockSize, r*blockSize, blockSize, blockSize);
            //MessageBox.Show(r.ToString() + c.ToString());
        }

        /*
        public void fillSquare(MyGraphics myGraphics)
        {
            int maxWidth = myGraphics.size[0];
            int maxHeight = myGraphics.size[1];
            int scalar = myGraphics.scalar;
            int a = 0;
            int b = 0;
            while (a*scalar <= maxWidth - scalar)
            {
                b = 0;
                List<Block>temp = new List<Block>();
                while(b*scalar <= maxHeight - scalar)
                {
                    Block pnn = new Block(items[0], a*scalar, b*scalar, scalar, scalar);
                    temp.Add(pnn);
                    b++;
                }
                grid.Add(temp);
                a++;
            }
        }
        */
    };

    public class Cursor
    {
        public int x, y;
        public int xSrc, ySrc, xDst, yDst;
        public int maxLen;
        public Color color;
        public int radius;

        public Cursor(int xhero, int yhero)
        {
            xSrc = xhero;
            ySrc = yhero;
            maxLen = 250;
            color = Color.FromArgb(128, Color.White);
            xDst = xhero;
            yDst = yhero;
            radius = 30;
            x = xhero;
            y = yhero;
        }

        public void updatePosition(int xhero, int yhero, int ex, int ey)
        {
            xDst = ex;
            yDst = ey;
            double dx = xhero - ex;
            double dy = yhero - ey;
            double distance = Math.Sqrt(dx*dx + dy*dy);
            if (distance > maxLen)
            {
                double unitX = dx / distance;
                double unitY = dy / distance;

                x = (int)(xhero - unitX * maxLen);
                y = (int)(yhero - unitY * maxLen);
            }
            else
            {
                x = ex;
                y = ey;
            }
        }
    }
    
    public class PhysicObject
    {
        public int x, y, mass;
        public double gravityForce, minGravityForce;
        public bool isGravityApplied;
        public int w, h;
    }
    public class Enemy : PhysicObject
    {
        public String type;
        public Bitmap skin;
        public String state;
        public bool isMoving;
        public bool isAttacking;
        public int attackCooldown;
        public int maxAttackTime;
        public int curState;
        public int dir;
        public bool isJumping;
        public int damage;
        public int health;
        public int moveTime;
        public int stallTime;
        public int range;
        public bool canShoot;
        public int isOnFire;

        public Enemy(String type, int r, int c, MyGraphics gr)
        {
            x = c * gr.scalar;
            y = r * gr.scalar;
            curState = 0;
            state = "..\\..\\..\\..\\..\\assets\\enemy\\" + type + "\\" + type;
            skin = new Bitmap(state + curState.ToString() + ".png");
            w = gr.scalar;
            h = 2 * gr.scalar;
            isMoving = false;
            mass = 3;
            isGravityApplied = true;
            gravityForce = 50;
            minGravityForce = 50;
            isJumping = false;
            this.type = type;
            if(type == "zombie")
                damage = 5;
            if(type == "skeleton")
                damage = 10;
            if (type == "creeper")
                damage = 40;
            health = 100;
            isAttacking = false;
            attackCooldown = 20;
            maxAttackTime = 20;
            moveTime = 10;
            stallTime = 0;
            dir = 1;
            range = 5*gr.scalar;
            canShoot = false;
            isOnFire = 0;
        }

        public void move(int val)
        {
            x += val;
        }

        public bool isWithinRange(Hero hero)
        {
            double dx = (hero.x+hero.w/2) - (x+w/2);
            double dy = (hero.y+hero.h/2) - (y+h/3);
            double magnitude = Math.Sqrt(dx*dx + dy*dy);
            if(magnitude < range)
            {
                return true;
            }
            return false;
        }
    }

    public class Hero: PhysicObject
    {
        public Bitmap skin;
        public String state;
        public bool isMoving;
        public bool isRunning;
        public int curState;
        public int dir;
        public int frame;
        public bool isJumping;
        public int rush;
        public double rushRecover;
        public int damage;
        public List<Item> hotbar;
        public int health;
        public int isOnFire;
        public Hero(int r, int c, MyGraphics gr)
        {
            x = c*gr.scalar;
            y = r*gr.scalar;
            curState = 0;
            state = "..\\..\\..\\..\\..\\assets\\player\\Steve";
            skin = new Bitmap(state+curState.ToString()+".png");
            w = gr.scalar;
            h = 2*gr.scalar;
            isMoving = false;
            isRunning = false;
            mass = 2;
            isGravityApplied = true;
            gravityForce = 50;
            minGravityForce = 50;
            isJumping = false;
            rush = 20;
            rushRecover = 0;
            damage = 10;
            hotbar = new List<Item>();
            health = 100;
            isOnFire = 0;
        }

        public void move(int val)
        {
            x += val;
        }

        public void take(Item item, int amount)
        {
            for(int i=0; i<hotbar.Count; i++)
            {
                if (hotbar[i].name == item.name)
                {
                    if (hotbar[i].quantity < item.stack_max)
                    {
                        int dif = hotbar[i].stack_max - hotbar[i].quantity;
                        if(amount > dif)
                        {
                            hotbar[i].quantity += dif;
                        }
                        else
                        {
                            dif = amount;
                            hotbar[i].quantity += dif;
                        }
                        amount -= dif;
                    }
                }
            }
            while(amount > 0)
            {
                Item pnn = new Item(item);
                if(amount >= pnn.stack_max)
                {
                    pnn.quantity = pnn.stack_max;
                    amount -= pnn.stack_max;
                }
                else
                {
                    pnn.quantity = amount;
                    amount = 0;
                }
                hotbar.Add(pnn);
            }
        }
    }

    public class Projectile: Item
    {
        public Vector2 dir;
        public List<double> speed;
    }

    public class Physics
    {
        public void applyGravity(PhysicObject obj, int maxYDown)
        {
            if (obj.isGravityApplied)
            {
                int dy = (int)(0.5 * (obj.mass * obj.gravityForce));
                if(dy >= maxYDown)
                {
                    obj.y += maxYDown;
                    obj.gravityForce = obj.minGravityForce;
                    //MessageBox.Show("hi");
                    return;
                }
                obj.y += dy;
                obj.gravityForce += obj.gravityForce;
                if(obj.gravityForce > obj.minGravityForce*15)
                    obj.gravityForce = obj.minGravityForce * 15;
            }
        }

        public Projectile launchProjectile(Item obj, Vector2 dir, Vector2 src)
        {
            Projectile projectile = new Projectile();
            projectile.curState = obj.curState;
            projectile.name = obj.name;
            projectile.x = (int)src.x;
            projectile.y = (int)src.y;
            projectile.w = obj.w;
            projectile.h = obj.h;
            projectile.skin = obj.skin;
            projectile.states = obj.states;
            
            //MessageBox.Show(projectile.x.ToString());

            projectile.dir = dir;
            projectile.speed = new List<double>();
            projectile.speed.Add(70.0);
            projectile.speed.Add(70.0);
            return projectile;
        }

        public void moveProjectile(Projectile obj, int x_val, int y_val)
        {
            obj.x += x_val;
            obj.y += y_val;
            obj.dir.y += 0.1f;
        }
    }

    public class Element
    {
        public String name;
        public List<int> x;
        public List<int> y;
        public List<int> w;
        public List<int> h;
        public List<Bitmap> skins;
        public List<String> state;
        public int cur;
        public Element()
        {
            x = new List<int>();
            y = new List<int>();
            w = new List<int>();
            h = new List<int>();
            skins = new List<Bitmap>();
            state = new List<String>();
        }
    }
    public class HUD
    {
        public List<Element> elements;
        public HUD(int cw, int ch)
        {
            // Healthbar
            elements = new List<Element>();
            Element pnn = new Element();
            pnn.name = "Health";
            pnn.state.Add("..\\..\\..\\..\\..\\assets\\heart\\full.png");
            pnn.state.Add("..\\..\\..\\..\\..\\assets\\heart\\full_blinking.png");
            pnn.state.Add("..\\..\\..\\..\\..\\assets\\heart\\half.png");
            pnn.state.Add("..\\..\\..\\..\\..\\assets\\heart\\half_blinking.png");
            pnn.x.Add(cw / 2 - (120 / 2) - (120 * 4));
            pnn.y.Add(ch-120-40-20);
            pnn.w.Add(40);
            pnn.h.Add(40);
            pnn.skins.Add(new Bitmap(pnn.state[0]));
            for (int i=1; i<10; i++)
            {
                pnn.x.Add(pnn.x[0] + (pnn.w[0] * i));
                pnn.y.Add(pnn.y[0]);
                pnn.w.Add(pnn.w[0]);
                pnn.h.Add(pnn.h[0]);
                pnn.skins.Add(new Bitmap(pnn.state[0]));
            }
            pnn.cur = 9;
            elements.Add(pnn);

            // Stamina
            pnn = new Element();
            pnn.name = "Stamina";
            pnn.state.Add("..\\..\\..\\..\\..\\assets\\stamina\\food_full.png");
            pnn.state.Add("..\\..\\..\\..\\..\\assets\\stamina\\food_half.png");
            pnn.state.Add("..\\..\\..\\..\\..\\assets\\stamina\\food_empty.png");
            pnn.x.Add(cw / 2 + 140);
            pnn.y.Add(ch - 120 - 40 - 20);
            pnn.w.Add(40);
            pnn.h.Add(40);
            pnn.skins.Add(new Bitmap(pnn.state[0]));
            for (int i = 1; i < 10; i++)
            {
                pnn.x.Add(pnn.x[0] + (pnn.w[0] * i));
                pnn.y.Add(pnn.y[0]);
                pnn.w.Add(pnn.w[0]);
                pnn.h.Add(pnn.h[0]);
                pnn.skins.Add(new Bitmap(pnn.state[0]));
            }
            pnn.cur = 9;
            elements.Add(pnn);


            // Hotbar 
            pnn = new Element();
            pnn.name = "Hotbar";
            pnn.state.Add("..\\..\\..\\..\\..\\assets\\hotbar\\hotbar.png");
            pnn.state.Add("..\\..\\..\\..\\..\\assets\\hotbar\\hotbar_selection.png");
            pnn.x.Add(cw/2-(120/2)-(120*4));
            pnn.y.Add(ch-120);
            pnn.w.Add(120);
            pnn.h.Add(120);
            pnn.skins.Add(new Bitmap(pnn.state[0]));
            for (int i = 1; i < 9; i++)
            {
                pnn.x.Add(pnn.x[0] + (pnn.w[0] * i));
                pnn.y.Add(pnn.y[0]);
                pnn.w.Add(pnn.w[0]);
                pnn.h.Add(pnn.h[0]);
                pnn.skins.Add(new Bitmap(pnn.state[0]));
            }
            pnn.cur = 0;
            // 10 = current selection
            pnn.x.Add(pnn.x[pnn.cur]-5);
            pnn.y.Add(pnn.y[pnn.cur]-5);
            pnn.w.Add(pnn.w[pnn.cur]+10);
            pnn.h.Add(pnn.h[pnn.cur]+10);
            pnn.skins.Add(new Bitmap(pnn.state[1]));
            elements.Add(pnn);
        }
    }

    public class Vector2
    {
        public double x, y;
        public Vector2(double x, double y)
        {
            this.x = x; this.y = y;
        }
    }

    public class Effect
    {
        public List<int> x, y;
        public int w, h;
        public Bitmap skin;
        public String state;

        public Effect(int xx, int yy)
        {
            x = new List<int>();
            y = new List<int>();
            x.Add(xx);
            y.Add(yy);
            w = 72;
            h = 72;
            state = "..\\..\\..\\..\\..\\assets\\effect\\dither.png";
            skin = new Bitmap(state);
        }
        public void updateEffect(int max_health, int health)
        {
            int dif = (max_health - health)/10;
            if ((dif+1) % (max_health/40) == 0)
            {
                if(x.Count % 2 == 0)
                {
                    x.Add(x[0]);
                    if(y.Count / 2 == 1)
                    {
                        y.Add(y[0]+h);
                    }
                    else
                    {
                        y.Add(y[0]);
                    }
                }
                else
                {
                    x.Add(x[0]+w);
                    if (y.Count / 2 == 1)
                    {
                        y.Add(y[0] + h);
                    }
                    else
                    {
                        y.Add(y[0]);
                    }
                }
            }
        }

    }

    public partial class Game : Form
    {
        MyGraphics myGraphics;
        int ScrlX, ScrlY;
        Tiles tiles;
        Bitmap off;
        Graphics g;
        HUD hud;
        Hero hero;
        int startingBlockLevel;
        Timer timer;
        bool isRightPressed = false;
        bool isLeftPressed = false;
        bool isShiftPressed = false;
        bool isSpacePressed = true;
        bool isLeftMousePressed = false;
        Cursor cursor;
        WeatherEffect weather;
        Physics physics;
        Rectangle rushBar;
        int lastHitR;
        int lastHitC;
        Effect? effect;
        List<Item> items;
        List<Projectile> projectiles;
        List<Enemy> enemies;
        List<List<bool>> zombieBlocks;
        List<Lava> lavas;
        List<Ladder> ladders;
        bool isInLadder;
        bool isUpPressed;
        bool isDownPressed;

        public Game()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(0, 0); // Top-left corner
            this.FormBorderStyle = FormBorderStyle.FixedDialog; // Fixed size with title bar
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.ControlBox = true; // Shows the close button
            this.Load += Game_load;
        }

        public int[] getBlockNumberFromPosition(int y, int x)
        {
            int[] blockNumber = new int[2];
            blockNumber[0] = y / myGraphics.scalar;
            blockNumber[1] = x / myGraphics.scalar;
            return blockNumber;
        }

        public int min(int a, int b)
        {
            if (a < b)
                return a;
            return b;
        }
        public int max(int a, int b)
        {
            if (a > b)
                return a;
            return b;
        }

        private void Game_MouseDown(object? sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                isLeftMousePressed = true;
            }
            if(e.Button == MouseButtons.Right)
            {
                placeBlock();
            }
            DrawDubb();
        }
        private void Game_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isLeftMousePressed = false;
            }
        }


        public void destroyBlock()
        {
            if(hero.curState == 0)
            {
                hero.curState = 7;
            }
            else
            {
                hero.curState = 0;
            }
            hero.skin = new Bitmap(hero.state + hero.curState.ToString() + ".png");

            // Check if hitting zombie
            Vector2 prevVecZombie = new Vector2(hero.x + hero.w / 2, hero.y + hero.h / 3);
            Vector2 posZombie = DDA_Zombie(ref prevVecZombie);
            if (prevVecZombie != posZombie)
            {
                int[] arr = getBlockNumberFromPosition((int)posZombie.y, (int)posZombie.x);
                if (zombieBlocks[arr[0]][arr[1]])
                {
                    for(int i=0; i<enemies.Count; i++)
                    {
                        int[] curZombiePosStart = getBlockNumberFromPosition(enemies[i].y+5, enemies[i].x+5);
                        int[] curZombiePosEnd = getBlockNumberFromPosition(enemies[i].y + enemies[i].h-10, enemies[i].x + enemies[i].w-10);
                        if (arr[0] >= curZombiePosStart[0] && arr[0] <= curZombiePosEnd[0])
                        {
                            if (arr[1] >= curZombiePosStart[1] && arr[1] <= curZombiePosEnd[1])
                            {
                                // Valid Hit for Zombie i
                                enemies[i].health -= hero.damage;
                                if (enemies[i].health <= 0)
                                {
                                    int[] temp = getBlockNumberFromPosition(enemies[i].y, enemies[i].x);
                                    int[] temp2 = getBlockNumberFromPosition(enemies[i].y + enemies[i].h, enemies[i].x + enemies[i].w);
                                    for(int a = temp[0]; a <= temp2[0]; a++)
                                    {
                                        for(int b = temp[1]; b<= temp[1]; b++)
                                        {
                                            zombieBlocks[a][b] = false;
                                        }
                                    }
                                    enemies.RemoveAt(i);
                                    //EXP exp = new EXP(50);
                                    //dropLoot(exp);
                                    return;
                                }
                                // Bounce enemy off
                                if (hero.x + hero.w/2 < enemies[i].x + enemies[i].w/2)
                                {
                                    int step = isCollidingRight(enemies[i], myGraphics.scalar / 2 + 60);
                                    if (step >= 0)
                                    {
                                        enemies[i].move(step);
                                    }
                                }
                                else
                                {
                                    int step = isCollidingLeft(enemies[i], -myGraphics.scalar / 2 - 60);
                                    if (step <= 0)
                                    {
                                        enemies[i].move(step);
                                    }
                                }

                                if (lastHitR > 0 && lastHitC > 0)
                                {
                                    if (tiles.grid[lastHitR][lastHitC] != null)
                                    {
                                        tiles.grid[lastHitR][lastHitC].health = tiles.grid[lastHitR][lastHitC].max_health;
                                    }
                                }
                                if (tiles.grid[arr[0]][arr[1]] != null)
                                {
                                    tiles.grid[arr[0]][arr[1]].health = tiles.grid[arr[0]][arr[1]].max_health;
                                }
                                lastHitR = -1;
                                lastHitC = -1;
                                effect = null;
                                return;
                            }
                        }
                    }
                }
            }

            Vector2 prevVec = new Vector2(hero.x + hero.w / 2, hero.y + hero.h / 3);
            Vector2 pos = DDA(ref prevVec);

            if (pos != prevVec)
            {
                int[] arr = getBlockNumberFromPosition((int)pos.y, (int)pos.x);
                int r = arr[0];
                int c = arr[1];
                if(r != lastHitR || c != lastHitC)
                {
                    if(lastHitC > 0 && lastHitR > 0)
                    {
                        if (tiles.grid[lastHitR][lastHitC]!= null)
                        {
                            tiles.grid[lastHitR][lastHitC].health = tiles.grid[lastHitR][lastHitC].max_health;
                        }
                    }
                }
                if (c > 0 && r > 0)
                {
                    if (tiles.grid[r][c] != null)
                    {
                        if (tiles.grid[r][c].health == tiles.grid[r][c].max_health)
                            effect = new Effect(c * myGraphics.scalar, r * myGraphics.scalar);
                        else
                            if(effect != null)
                                effect.updateEffect(tiles.grid[r][c].max_health, tiles.grid[r][c].health);
                        tiles.grid[r][c].health -= hero.damage;
                        
                        if(tiles.grid[r][c].health <= 0)
                        {
                            // block destroyed
                            hero.take(items[tiles.grid[r][c].id], 1);
                            tiles.grid[r][c] = null;
                            effect = null;
                        }
                        lastHitR = r;
                        lastHitC = c;
                    }
                }
            }
            else
            {
                lastHitC = -1;
                lastHitR = -1;
                effect = null;
            }
        }

        public void placeBlock()
        {
            Vector2 prevVec = new Vector2(hero.x + hero.w / 2, hero.y + hero.h / 3);
            Vector2 pos = DDA(ref prevVec);

            if (pos!=prevVec)
            {
                int[] arr = getBlockNumberFromPosition((int)prevVec.y, (int)prevVec.x);
                int r = arr[0];
                int c = arr[1];
                int[] h_pos = getBlockNumberFromPosition(hero.y, hero.x+5);
                if ((h_pos[0] != r && (hero.y + hero.h - 1) / myGraphics.scalar != r && (hero.y + hero.h/2) / myGraphics.scalar != r) || (h_pos[1] != c && (hero.x + hero.w-10) / myGraphics.scalar != c))
                    if (tiles.grid[r][c] == null && !zombieBlocks[r][c])
                    {
                        hero.skin = new Bitmap(hero.state + "7.png");
                        if (hud.elements[2].cur < hero.hotbar.Count)
                            if (hero.hotbar[hud.elements[2].cur].placementBehavior == "place")
                            {
                                tiles.addBlock(r, c, hero.hotbar[hud.elements[2].cur]);
                                hero.hotbar[hud.elements[2].cur].quantity--;
                            }
                    }

                
            }
            if(hud.elements[2].cur < hero.hotbar.Count && hero.hotbar[hud.elements[2].cur].placementBehavior == "throw")
            {
                Vector2 dir;
                Vector2 src = new Vector2(hero.x+hero.w/2, hero.y+hero.h/4);
                Random random = new Random();
                int accuracy = random.Next(-cursor.radius, cursor.radius);
                double magnitude = Math.Sqrt(Math.Abs((cursor.x+accuracy-src.x) * (cursor.x + accuracy - src.x) + (cursor.y + accuracy - src.y) * (cursor.y + accuracy - src.y)));
                if (magnitude > 0 && !double.IsNaN(magnitude))
                {
                    hero.skin = new Bitmap(hero.state + "7.png");
                    dir = new Vector2((cursor.x + accuracy - src.x) / magnitude, (cursor.y + accuracy - src.y) / magnitude);
                    //MessageBox.Show(dir.x.ToString() + " " + dir.y.ToString());
                    Item pnn = items[hero.hotbar[hud.elements[2].cur].id];
                    pnn.quantity = 1;
                    hero.hotbar[hud.elements[2].cur].quantity--;
                    projectiles.Add(physics.launchProjectile(pnn, dir, src));
                }
            }
            if(hero.hotbar[hud.elements[2].cur].quantity == 0)
            {
                hero.hotbar.RemoveAt(hud.elements[2].cur);
            }
        }

        public Vector2 DDA(ref Vector2 prev)
        {
            int accuracy = 2;
            Vector2 Ha = new Vector2(hero.x+hero.w/2, hero.y+hero.h/3);
            Vector2 Hb = new Vector2(Ha.x+(cursor.xDst-Ha.x)*4*myGraphics.scalar,Ha.y+(cursor.yDst-Ha.y)*4*myGraphics.scalar);

            // hat7arak bel scalar fe direction el dir vector
            Vector2 dir = new Vector2(Hb.x - Ha.x, Hb.y - Ha.y);
            double magnitude = Math.Sqrt(dir.x * dir.x + dir.y * dir.y);

            if (magnitude == 0f)
                return Ha;

            dir = new Vector2(dir.x / magnitude, dir.y / magnitude);
            
            Vector2 curPos = new Vector2(Ha.x, Ha.y);
            float distanceTravelled = 0f;
            while(distanceTravelled < 5f * myGraphics.scalar)
            {
                int[] arr = getBlockNumberFromPosition((int)curPos.y, (int)curPos.x);
                if (isTile(arr[0], arr[1]))
                {
                    return curPos;
                }
                prev.x = curPos.x;
                prev.y = curPos.y;
                curPos.x += dir.x * accuracy;
                curPos.y += dir.y * accuracy;
                distanceTravelled += accuracy;
            }
            return prev;
        }

        public Vector2 DDA_Zombie(ref Vector2 prev)
        {
            int accuracy = 2;
            Vector2 Ha = new Vector2(hero.x + hero.w / 2, hero.y + hero.h / 3);
            Vector2 Hb = new Vector2(Ha.x + (cursor.xDst - Ha.x) * 4 * myGraphics.scalar, Ha.y + (cursor.yDst - Ha.y) * 4 * myGraphics.scalar);

            // hat7arak bel scalar fe direction el dir vector
            Vector2 dir = new Vector2(Hb.x - Ha.x, Hb.y - Ha.y);
            double magnitude = Math.Sqrt(dir.x * dir.x + dir.y * dir.y);

            if (magnitude == 0f)
                return Ha;

            dir = new Vector2(dir.x / magnitude, dir.y / magnitude);

            Vector2 curPos = new Vector2(Ha.x, Ha.y);
            float distanceTravelled = 0f;
            while (distanceTravelled < 5f * myGraphics.scalar)
            {
                int[] arr = getBlockNumberFromPosition((int)curPos.y, (int)curPos.x);
                if (zombieBlocks[arr[0]][arr[1]])
                {
                    return curPos;
                }
                prev.x = curPos.x;
                prev.y = curPos.y;
                curPos.x += dir.x * accuracy;
                curPos.y += dir.y * accuracy;
                distanceTravelled += accuracy;
            }
            return prev;
        }

        public bool isTile(int r, int c)
        {
            return tiles.grid[r][c] != null;
        }

        private void Game_load(object? sender, EventArgs e)
        {
            myGraphics = new MyGraphics();
            this.Width = myGraphics.size[0];
            this.Height = myGraphics.size[1];
            
            off = new Bitmap(this.ClientSize.Width, this.ClientSize.Height);
            g = this.CreateGraphics();
            
            ScrlX = 10*myGraphics.size[0];
            ScrlY = 10*myGraphics.size[1];
            physics = new Physics();
            hud = new HUD(this.Width,this.Height-144);

            startingBlockLevel = 4;
            tiles = new Tiles(myGraphics);

            // Create zombie blocks
            zombieBlocks = new List<List<bool>>();
            for (int i = 0; i < 110 * (myGraphics.size[1] / myGraphics.scalar); i++)
            {
                List<bool> row = new List<bool>();
                for (int j = 0; j < 110 * 21; j++)
                {
                    row.Add(false);
                }
                zombieBlocks.Add(row);
            }

            items = new List<Item>();
            LoadAllItems();
            LoadGround();

            weather = new WeatherEffect(0);
            enemies = new List<Enemy>();

            int[] pos = getBlockNumberFromPosition(ScrlY + this.Height-(startingBlockLevel*myGraphics.scalar), ScrlX + this.Width / 2);
            hero = new Hero(pos[0], pos[1], myGraphics);
            enemies.Add(new Enemy("zombie", pos[0], pos[1] + 6, myGraphics));
            hero.take(items[0], 10);
            hero.take(items[1], 64);
            hero.take(items[2], 14);

            cursor = new Cursor(hero.x + hero.w / 2, hero.y + hero.h / 3);
            rushBar = new Rectangle(this.Width / 2 - 540, this.Height - 144 - 135, hero.rush * 1080 / 20, 5);
            projectiles = new List<Projectile>();

            // Create lava
            lavas = new List<Lava>();
            Lava pnn = new Lava(items[3], ScrlX + 1300, ScrlY + 10, myGraphics.scalar, myGraphics.scalar);
            lavas.Add(pnn);

            // Create ladders
            ladders = new List<Ladder>();
            for(int i=0; i<5; i++)
            {
                Ladder pnnL = new Ladder(items[4], ScrlX + 700, ScrlY + (i * myGraphics.scalar), myGraphics.scalar, myGraphics.scalar);
                ladders.Add(pnnL);
            }

            isInLadder = false;
            isUpPressed = false;
            isDownPressed = false;

            lastHitR = -1;
            lastHitC = -1;
            
            this.Paint += Game_Paint;
            this.MouseDown += Game_MouseDown;
            this.MouseUp += Game_MouseUp;
            this.KeyDown += Game_KeyDown;
            this.KeyUp += Game_KeyUp;
            this.MouseMove += Game_MouseMove;
            this.MouseWheel += Game_MouseWheel;

            timer = new Timer();
            timer.Interval = 1000 / myGraphics.fps;
            timer.Tick += Timer_Tick;
            timer.Start();

            Timer movementTimer = new Timer();
            movementTimer.Interval = 1000 / (2*myGraphics.fps);
            movementTimer.Tick += MovementTimer_Tick;
            movementTimer.Start();

            //MessageBox.Show(hero.y.ToString() + " " + hero.x.ToString());
            DrawDubb();
        }

        public void respawn()
        {  
            ScrlX = 10 * myGraphics.size[0];
            ScrlY = 10 * myGraphics.size[1];
            int[] pos = getBlockNumberFromPosition(ScrlY + this.Height - (startingBlockLevel * myGraphics.scalar), ScrlX + this.Width / 2);
            hero = new Hero(pos[0], pos[1], myGraphics);
            hud.elements[0].cur = 9;
            hud.elements[1].cur = 9;
            hud.elements[2].cur = 0;
            cursor = new Cursor(hero.x + hero.w / 2, hero.y + hero.h / 3);
            rushBar = new Rectangle(this.Width / 2 - 540, this.Height - 144 - 135, hero.rush * 1080 / 20, 5);
            projectiles = new List<Projectile>();

            lastHitR = -1;
            lastHitC = -1;
        }

        private void Game_MouseWheel(object? sender, MouseEventArgs e)
        {
            int scrollSensitivity = 2;
            hud.elements[2].cur += e.Delta / (100 * scrollSensitivity)+9;
            hud.elements[2].cur %= 9;
            hud.elements[2].x[9] = hud.elements[2].x[hud.elements[2].cur] - 5;
            hud.elements[2].y[9] = hud.elements[2].y[hud.elements[2].cur] - 5;
        }

        private void Game_MouseMove(object? sender, MouseEventArgs e)
        {
            cursor.updatePosition(hero.x + hero.w / 2, hero.y + hero.h / 3, e.X-cursor.radius/2 + ScrlX, e.Y-cursor.radius/2+ScrlY);
        }

        public void updateLava()
        {
            List<int> hero_r = new List<int>();
            List<int> hero_c = new List<int>();
            int[] hero_pos_start = getBlockNumberFromPosition(hero.y, hero.x);
            int[] hero_pos_end = getBlockNumberFromPosition(hero.y+hero.h, hero.x+hero.w);
            for (int i = hero_pos_start[0]; i<= hero_pos_end[0]; i++)
            {
                hero_r.Add(i);
            }
            for (int i = hero_pos_start[1]; i <= hero_pos_end[1]; i++)
            {
                hero_c.Add(i);
            }

            for (int i=0; i<lavas.Count; i++)
            {
                bool in_col = hero_c.Contains(lavas[i].x / myGraphics.scalar);
                bool in_row = false;
                int r = lavas[i].y / myGraphics.scalar;
                int c = lavas[i].x / myGraphics.scalar;
                lavas[i].curState++;
                lavas[i].curState %= lavas[i].states.Count;
                int curr = lavas[i].curState;
                lavas[i].path = new List<Block>();
                int maxDown = 15;
                while (r+2< tiles.row && tiles.grid[r][c] == null && maxDown-->0)
                {
                    Block pnn = new Block(lavas[i], lavas[i].x, r * myGraphics.scalar, lavas[i].w, lavas[i].h);
                    pnn.curState = curr;
                    pnn.skin = new Bitmap(lavas[i].states[curr]);
                    // check if in same row as hero
                    in_row = hero_r.Contains(r);
                    if(in_row && in_col)
                    {
                        // hit hero
                        hero.isOnFire += 5;
                        if(hero.x > lavas[i].x)
                        {
                            int val = isCollidingRight(hero, myGraphics.scalar/2);
                            hero.x += val;
                            ScrlX += val;
                        }
                        else
                        {
                            int val = isCollidingLeft(hero, -myGraphics.scalar/2);
                            hero.x += val;
                            ScrlX += val;
                        }
                            in_row = false;
                    }

                    // check if in same row as enemy
                    if (zombieBlocks[r][c])
                    {
                        for(int k=0; k < enemies.Count; k++)
                        {
                            List<int> enemy_r = new List<int>();
                            List<int> enemy_c = new List<int>();
                            int[] enemy_pos_start = getBlockNumberFromPosition(enemies[k].y, enemies[k].x);
                            int[] enemy_pos_end = getBlockNumberFromPosition(enemies[k].y + enemies[k].h, enemies[k].x + enemies[k].w);
                            for (int f = enemy_pos_start[0]; f <= enemy_pos_end[0]; f++)
                            {
                                enemy_r.Add(f);
                            }
                            for (int f = enemy_pos_start[1]; f <= enemy_pos_end[1]; f++)
                            {
                                enemy_c.Add(f);
                            }
                            if (enemy_c.Contains(c) && enemy_r.Contains(r))
                            {
                                // hit enemy k
                                enemies[k].isOnFire += 5;
                                if (enemies[k].x > lavas[i].x)
                                {
                                    int val = isCollidingRight(enemies[k], myGraphics.scalar / 2);
                                    enemies[k].x += val;
                                }
                                else
                                {
                                    int val = isCollidingLeft(enemies[k], -myGraphics.scalar / 2);
                                    enemies[k].x += val;
                                }
                            }
                        }
                    }


                    curr++;
                    curr %= lavas[i].states.Count;
                    r++;
                    lavas[i].path.Add(pnn);
                }
                
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            // update lava
            updateLava();
            if (hero.isOnFire>0)
            {
                heroDamaged(5);
                if (hud.elements[0].cur == -1)
                {
                    respawn();
                    return;
                }
                hero.isOnFire--;
            }

            int[] pos = getBlockNumberFromPosition(hero.y + hero.h - (myGraphics.scalar), hero.x + hero.w / 2);
            int row = pos[0];
            int col = pos[1];
            int extra_col_front = (hero.x + myGraphics.scalar / 8 + hero.w / 2) / myGraphics.scalar;
            int extra_col_back = (hero.x + myGraphics.scalar / 4) / myGraphics.scalar;

            int dx = hero.x;
            int dy = hero.y;

            // apply gravity on hero
            int maxYDown = isCollidingBottom(hero, (int)(hero.mass * hero.gravityForce/2));
            if (maxYDown > 0)
            {
                hero.skin = new Bitmap(hero.state + "2.png");
                physics.applyGravity(hero, maxYDown);
            }
            else
            {
                hero.skin = new Bitmap(hero.state + "0.png");
                hero.gravityForce = hero.minGravityForce;
                hero.isJumping = false;
                isSpacePressed = false;
            }
            dx = hero.x - dx;
            dy = hero.y - dy;
            ScrlX += dx;
            ScrlY += dy;
            cursor.x += dx;
            cursor.y += dy;
            
            this.Text = hero.health.ToString() + "HP - " + hud.elements[0].cur.ToString() + " Fire: " + hero.isOnFire.ToString();
            // Update Health
            for (int heart_num = 0; heart_num < hud.elements[0].cur; heart_num++)
            {
                hud.elements[0].skins[heart_num] = new Bitmap(hud.elements[0].state[0]); // full_heart
            }
            // if above 5s full
            if ((hero.health - 1) % 10 >= 5)
                hud.elements[0].skins[hud.elements[0].cur] = new Bitmap(hud.elements[0].state[0]); // full
            else
                hud.elements[0].skins[hud.elements[0].cur] = new Bitmap(hud.elements[0].state[2]); // half

            if (!hero.isMoving)
            {
                if (cursor.x > hero.x + hero.w / 2)
                    hero.dir = 1;
                else
                    hero.dir = -1;
            }
            cursor.updatePosition(hero.x + hero.w / 2, hero.y + hero.h / 3, cursor.x, cursor.y);
            if (!isLeftMousePressed)
            {
                if (lastHitC > 0 && lastHitR > 0)
                    if (tiles.grid[lastHitR][lastHitC] != null)
                    {
                        tiles.grid[lastHitR][lastHitC].health = tiles.grid[lastHitR][lastHitC].max_health;
                    }
                effect = null;
            }
            else
            {
                destroyBlock();
            }


            // ENEMY
            for (int i=0; i<enemies.Count; i++)
            {
                // check if is on fire
                if (enemies[i].isOnFire > 0)
                {
                    enemies[i].health -= 5;
                    enemies[i].isOnFire--;
                }

                // resetting zombie block
                int[] zombie_start_location = getBlockNumberFromPosition(enemies[i].y+5, enemies[i].x+5);
                int[] zombie_end_location = getBlockNumberFromPosition(enemies[i].y + enemies[i].h-10, enemies[i].x + enemies[i].w-10);
                for(int r = zombie_start_location[0]; r <= zombie_end_location[0]; r++)
                {
                    for(int c = zombie_start_location[1]; c <= zombie_end_location[1]; c++)
                    {
                        zombieBlocks[r][c] = false;
                    }
                }


                enemies[i].isAttacking = enemies[i].isWithinRange(hero);
                

                // Apply gravity on enemies
                maxYDown = isCollidingBottom(enemies[i], (int)(enemies[i].mass * enemies[i].gravityForce / 2));
                if(maxYDown > 0)
                {
                    physics.applyGravity(enemies[i], maxYDown);
                    enemies[i].isJumping = true;
                    enemies[i].curState = 5;
                }
                else
                {
                    enemies[i].gravityForce = enemies[i].minGravityForce;
                    enemies[i].isJumping = false;
                }
                
                // Enemy movement
                if (!enemies[i].isAttacking)
                {
                    if (enemies[i].curState > 2)
                    {
                        // reset after attack
                        enemies[i].curState = 0;
                        enemies[i].stallTime = 10;
                        enemies[i].moveTime = 0;
                        enemies[i].isMoving = false;
                    }
                    
                    // move enemy
                    if (!enemies[i].isJumping)
                    { 
                        if (enemies[i].isMoving)
                        {
                            if (enemies[i].moveTime > 0)
                            {
                                int temp_dx = enemies[i].x;

                                if (enemies[i].dir > 0)
                                    enemies[i].move(isCollidingRight(enemies[i], myGraphics.scalar / 6));
                                else
                                    enemies[i].move(isCollidingLeft(enemies[i], -myGraphics.scalar / 6));

                                temp_dx = enemies[i].x - temp_dx;
                                if (temp_dx != 0)
                                {
                                    // movement happened
                                    // animate movement
                                    if (enemies[i].curState == 1)
                                    {
                                        enemies[i].curState = 2;
                                    }
                                    else
                                    {
                                        enemies[i].curState = 1;
                                    }
                                }
                                else
                                {
                                    enemies[i].curState = 0;
                                    enemies[i].moveTime = 1;
                                }
                                enemies[i].moveTime--;
                            }
                            else
                            {
                                enemies[i].isMoving = false;
                                enemies[i].curState = 0;
                                Random random = new Random();
                                enemies[i].stallTime = random.Next(20, 90); ;
                            }
                        }
                        // stall enemy
                        if (!enemies[i].isMoving)
                        {
                            enemies[i].curState = 0;
                            if (enemies[i].stallTime > 0)
                            {
                                enemies[i].stallTime--;
                            }
                            else
                            {
                                enemies[i].isMoving = true;
                                enemies[i].dir *= -1;
                                Random random = new Random();
                                enemies[i].moveTime = random.Next(5, 20);
                            }
                        }
                    }
                    enemies[i].canShoot = false;
                    enemies[i].attackCooldown = enemies[i].maxAttackTime;
                }
                else
                {
                    // enemy attacks hero
                    int temp_dx = 0;
                    bool enemyGreater = enemies[i].x + enemies[i].w / 2 > hero.x + hero.w / 2;
                    if (enemyGreater)
                        temp_dx = (hero.x + hero.w) - enemies[i].x;
                    else
                        temp_dx = hero.x - (enemies[i].x + enemies[i].w);
                    int temp_dy = (hero.y + hero.h / 2) - (enemies[i].y + enemies[i].h / 2);
                    int old_x = enemies[i].x;

                    int movementValue = min(Math.Abs(temp_dx), myGraphics.scalar / 4);
                    if (enemyGreater)
                        movementValue = isCollidingLeft(enemies[i], -movementValue);
                    else
                        movementValue = isCollidingRight(enemies[i], movementValue);

                    if ((enemyGreater && enemies[i].x - movementValue > hero.x + hero.w) || (!enemyGreater && enemies[i].x + enemies[i].w + movementValue < hero.x))
                    {
                        if (enemyGreater)
                        {
                            enemies[i].move(movementValue);
                            enemies[i].dir = -1;
                        }
                        else
                        {
                            enemies[i].move(movementValue);
                            enemies[i].dir = 1;
                        }
                        // far away so can't shoot
                        enemies[i].canShoot = false;
                        enemies[i].attackCooldown = enemies[i].maxAttackTime;
                        enemies[i].isMoving = true;
                    }
                    else
                    {
                        // start shooting
                        enemies[i].canShoot = true;
                        if ((enemies[i].y + enemies[i].h / 3) / myGraphics.scalar >= (hero.y+5) / myGraphics.scalar && (enemies[i].y + enemies[i].h / 3) / myGraphics.scalar <= (hero.y + hero.h-5) / myGraphics.scalar)
                        {
                            if (enemies[i].curState != 5)
                                enemies[i].curState = 0;
                            else
                                enemies[i].curState = 6;

                            if (enemies[i].attackCooldown == 0)
                            {
                                enemies[i].curState = 5;
                                if (enemies[i].type == "zombie" && hero.health > 0)
                                {
                                    heroDamaged(enemies[i].damage);
                                    if (hud.elements[0].cur == -1)
                                    {
                                        respawn();
                                        break;
                                    }
                                }
                            }

                            enemies[i].attackCooldown++;
                            enemies[i].attackCooldown %= enemies[i].maxAttackTime;
                            enemies[i].isMoving = false;
                        }
                    }


                    if (old_x == enemies[i].x && enemies[i].isAttacking && !enemies[i].canShoot)
                    {
                        // trying to move but stuck on X
                        // jump if possible
                        int t_dy = enemies[i].y;
                        if (!enemies[i].isJumping)
                        {
                            int desiredY = -enemies[i].h / 2;
                            int y_val = desiredY;
                            y_val = max(y_val, isCollidingTop(enemies[i], desiredY));
                            if (y_val == desiredY)
                            {
                                enemies[i].y += y_val;
                                enemies[i].isJumping = true;
                            }

                        }
                        int t_dx = enemies[i].x;
                        if (temp_dx > 0)
                        {
                            int step = isCollidingRight(enemies[i], myGraphics.scalar / 2);
                            if (step >= 0)
                            {
                                enemies[i].move(step);

                            }
                            enemies[i].dir = 1;
                        }
                        else if (temp_dx < 0)
                        {
                            int step = isCollidingLeft(enemies[i], -myGraphics.scalar / 2);
                            if (step <= 0)
                            {
                                enemies[i].move(step);

                            }

                            enemies[i].dir = -1;
                        }
                        if (t_dx == enemies[i].x)
                        {
                            if (t_dy != enemies[i].y)
                            {
                                enemies[i].y -= enemies[i].y - t_dy;
                                enemies[i].isJumping = false;
                            }
                        }
                    }
                    else
                    {
                        // movement happened
                        // animate movement
                        if (enemies[i].isMoving)
                        {
                            if (enemies[i].curState == 3)
                            {
                                enemies[i].curState = 4;
                            }
                            else
                            {
                                enemies[i].curState = 3;
                            }
                        }
                    }
                }
                enemies[i].skin = new Bitmap(enemies[i].state + enemies[i].curState + ".png");

                // updating zombie block
                zombie_start_location = getBlockNumberFromPosition(enemies[i].y + 5, enemies[i].x + 5);
                zombie_end_location = getBlockNumberFromPosition(enemies[i].y + enemies[i].h - 10, enemies[i].x + enemies[i].w - 10);
                for (int r = zombie_start_location[0]; r <= zombie_end_location[0]; r++)
                {
                    for (int c = zombie_start_location[1]; c <= zombie_end_location[1]; c++)
                    {
                        zombieBlocks[r][c] = true;
                    }
                }
            }

            // PROJECTILES
            for (int i = 0; i < projectiles.Count; i++)
            {
                int desiredX = (int)(projectiles[i].dir.x * projectiles[i].speed[0]);
                int x_val = desiredX;

                if (desiredX > 0)
                {
                    x_val = min(x_val, isCollidingRight(projectiles[i], desiredX));
                }
                else if (desiredX < 0)
                    x_val = max(x_val, isCollidingLeft(projectiles[i], desiredX));


                int desiredY = (int)(projectiles[i].dir.y * projectiles[i].speed[1]);
                int y_val = desiredY;

                if (desiredY > 0)
                {
                    y_val = min(y_val, isCollidingBottom(projectiles[i], desiredY));
                }
                else if (desiredY < 0)
                    y_val = max(y_val, isCollidingTop(projectiles[i], desiredY));

                for(int j=0; j<enemies.Count; j++)
                {
                    if (projectiles[i].x + projectiles[i].w >= enemies[j].x && projectiles[i].x + projectiles[i].w <= enemies[j].x + enemies[j].w)
                    {
                        if (projectiles[i].y + projectiles[i].h >= enemies[j].y && projectiles[i].y + projectiles[i].h <= enemies[j].y + enemies[j].h)
                        {
                            //MessageBox.Show("Hit");
                            projectiles[i].speed[0] = 0;
                            projectiles[i].speed[1] = 0;
                            // Hit zombie
                            enemies[j].health -= 5;
                            if (projectiles[i].dir.x > 0.0f)
                            {
                                int step = isCollidingRight(enemies[j], myGraphics.scalar/2 + 60);
                                if (step >= 0)
                                {
                                    enemies[j].move(step);
                                    
                                }
                            }
                            else if(projectiles[i].dir.x < 0.0f)
                            { 
                                int step = isCollidingLeft(enemies[j], -myGraphics.scalar/2 -60);
                                if (step <= 0)
                                {
                                    enemies[j].move(step);

                                }
                            }
                        }
                    }
                }
                

                if (x_val != desiredX || y_val != desiredY)
                {
                    // collision
                    projectiles[i].speed[0] = 0;
                    projectiles[i].speed[1] = 0;
                }
                physics.moveProjectile(projectiles[i], x_val, y_val);
                if (projectiles[i].speed[0] == 0 && projectiles[i].speed[1] == 0)
                {
                    projectiles.RemoveAt(i);
                    i--;
                }
            }
            
            DrawDubb();
        }

        public void heroDamaged(int val)
        {
            hero.health -= val;
            hud.elements[0].cur = ((hero.health + 9) / 10) - 1;
            if (hero.health <= 0)
                return;
            // blink hearts
            for (int heart_num = 0; heart_num < hud.elements[0].cur; heart_num++)
            {
                hud.elements[0].skins[heart_num] = new Bitmap(hud.elements[0].state[1]);
            }
            // if above 5s blink half
            if ((hero.health - 1) % 10 >= 5) // 90 -> 89%10 = 9    96 -> 95%10 = 5      94 -> 93%10 = 3
                hud.elements[0].skins[hud.elements[0].cur] = new Bitmap(hud.elements[0].state[3]); // half_blinked
            else
                hud.elements[0].skins[hud.elements[0].cur] = new Bitmap(hud.elements[0].state[1]); // full_blinked

            if (!hero.isJumping)
            {
                hero.isJumping = true;
                int jump_val = isCollidingTop(hero, -myGraphics.scalar / 2);
                hero.y += jump_val;
                ScrlY += jump_val;
            }
        }

        public int isCollidingRight(PhysicObject obj, int val)
        {
            int[] startBlock = getBlockNumberFromPosition(obj.y, obj.x + obj.w);
            int[] endBlock = getBlockNumberFromPosition(obj.y+obj.h-10, obj.x + obj.w + val);

            for (int col = startBlock[1]; col <= endBlock[1]; col++)
            {
                for (int row = startBlock[0]; row <= endBlock[0]; row++)
                {
                    if (tiles.grid[row][col] != null)
                    {
                        return (col * myGraphics.scalar) - (obj.x + obj.w);
                    }
                }
            }
            return val;
        }
        public int isCollidingLeft(PhysicObject obj, int val)
        {
            int[] startBlock = getBlockNumberFromPosition(obj.y, obj.x);
            int[] endBlock = getBlockNumberFromPosition(obj.y+obj.h-10, obj.x + val);

            for (int col = startBlock[1]; col >= endBlock[1]; col--)
            {
                for (int row = startBlock[0]; row <= endBlock[0]; row++)
                {
                    if (tiles.grid[row][col] != null)
                    {
                        return ((col + 1) * myGraphics.scalar) - obj.x;
                    }
                }
            }
            return val;
        }
        public int isCollidingTop(PhysicObject obj, int val)
        {
            int[] startBlock = getBlockNumberFromPosition(obj.y, obj.x+5);
            int[] endBlock = getBlockNumberFromPosition(obj.y + val, obj.x + obj.w-10);

            for (int row = startBlock[0]; row >= endBlock[0]; row--)
            {
                for (int col = startBlock[1]; col <= endBlock[1]; col++)
                {
                    if (tiles.grid[row][col] != null)
                    {
                        //MessageBox.Show("hit");
                        return ((row+1) * myGraphics.scalar) - obj.y;
                    }
                }
            }
            return val;
        }
        public int isCollidingBottom(PhysicObject obj, int val)
        {
            int[] startBlock = getBlockNumberFromPosition(obj.y, obj.x+7);
            int[] endBlock = getBlockNumberFromPosition(obj.y + obj.h + val, obj.x + obj.w - 14);

            for (int col = startBlock[1]; col <= endBlock[1]; col++)
            {
                for (int row = startBlock[0]; row <= endBlock[0]; row++)
                {
                    if (tiles.grid[row][col] != null)
                    {
                        return (row * myGraphics.scalar) - (obj.y + obj.h);
                    }
                }
            }
            return val;
        }

        private void MovementTimer_Tick(object? sender, EventArgs e)
        {
            if (isRightPressed || isLeftPressed)
            {
                animateMovement();
                DrawDubb();
            }
            else if (hero.isRunning || hero.isMoving)
            {
                hero.isRunning = false;
                if(hero.curState == 0)
                {
                    hero.isMoving = false;
                }
                else
                {
                    hero.curState--;
                    hero.skin = new Bitmap(hero.state + hero.curState.ToString() + ".png");
                }
                DrawDubb();
                
            }

            // update rush
            if (!hero.isRunning)
            {
                hero.rushRecover += 0.1 - ((int)(hero.rushRecover) / (21-hero.rush));
            }
            int rushHealAmount = 20 - hero.rush;
            if (hero.rushRecover >= rushHealAmount)
            {
                hero.rush += rushHealAmount;
                hero.rushRecover -= rushHealAmount;
            }
            else
            {
                hero.rush += (int)(hero.rushRecover);
                hero.rushRecover -= (int)(hero.rushRecover);
            }
            rushBar.Width = hero.rush * 1080 / 20;

            cursor.updatePosition(hero.x + hero.w / 2, hero.y + hero.h / 3, cursor.x, cursor.y);
        }

        public void animateMovement()
        {
            int dx = hero.x;
            int dy = hero.y;
            hero.isMoving = true;

            // Animate movement
            if (hero.isRunning)
            {
                if (hero.curState == 6)
                    hero.curState = 1;
                else
                    hero.curState = 6;
            }
            else
            {
                hero.curState++;
                hero.curState %= 7;
            }

            // Starts running when holding shift
            if (isShiftPressed && hero.rush > 0)
            {
                hero.isRunning = true;
            }
            else
            {
                hero.isRunning = false;
                isShiftPressed = false;
            }

            // Update skin frame
            hero.skin = new Bitmap(hero.state + hero.curState.ToString() + ".png");

            // Running behavior
            if (hero.isRunning)
            {
                if(isRightPressed)
                    hero.move(isCollidingRight(hero,myGraphics.scalar * 2));
                if(isLeftPressed)
                    hero.move(isCollidingLeft(hero, -myGraphics.scalar * 2));
                hero.rush -= 1;
                if (hero.rush < 0)
                    hero.rush = 0;
            }
            else
            {
                if(isRightPressed)
                    hero.move(isCollidingRight(hero, 70));
                if(isLeftPressed)
                    hero.move(isCollidingLeft(hero, -70));
            }
            dx = hero.x - dx;
            dy = hero.y - dy;
            ScrlX += dx;
            ScrlY += dy;
            cursor.x += dx;
            cursor.y += dy;
        }

        private void Game_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.D)
            {
                isRightPressed = true;
                //hero.curState = 0;
                hero.dir = 1;
            }
            else if (e.KeyCode == Keys.A)
            {
                isLeftPressed = true;
                //hero.curState = 0;
                hero.dir = -1;
            }else if(e.KeyCode == Keys.Space)
            {
                if (isSpacePressed == false)
                {
                    isSpacePressed = true;
                    if (!hero.isJumping)
                    {
                        hero.isJumping = true;
                        int jump_val = isCollidingTop(hero, (-3 * myGraphics.scalar/2));
                        hero.y += jump_val;
                        hero.isOnFire -= 4;
                        if (hero.isOnFire < 0)
                            hero.isOnFire = 0;

                        ScrlY += jump_val;
                        cursor.y += jump_val;
                        cursor.updatePosition(hero.x + hero.w / 2, hero.y + hero.h / 3, cursor.x, cursor.y);
                    }
                }
            }else if(e.KeyCode == Keys.P)
            {
                hero.isGravityApplied = !hero.isGravityApplied;
            }else if (e.KeyCode == Keys.W)
            {
                // go up
                isUpPressed = true;
            }else if (e.KeyCode == Keys.S)
            {
                // go down
                isDownPressed = true;
            }
            if (e.KeyCode == Keys.ShiftKey)
            {
                // Check if running is possible
                if (hero.rush > 0)
                {
                    isShiftPressed = true;
                }
                else
                {
                    isShiftPressed = false;
                }
            }
        }

        private void Game_KeyUp(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.D)
            {
                isRightPressed = false;
            }
            else if (e.KeyCode == Keys.A)
            {
                isLeftPressed = false;
            }
            else if (e.KeyCode == Keys.W)
            {
                // don't go up
                isUpPressed = false;
            }
            else if (e.KeyCode == Keys.S)
            {
                // don't go down
                isDownPressed = false;
            }
            if (e.KeyCode == Keys.ShiftKey)
            {
                isShiftPressed = false;
            }
        }
        public void LoadGround()
        {
            int cols = myGraphics.size[0]/myGraphics.scalar;
            for(int i=0; i<cols; i++)
            {
                int[] pos = getBlockNumberFromPosition(ScrlY + this.Height - ((startingBlockLevel-2) * myGraphics.scalar), ScrlX + (i*myGraphics.scalar));
                tiles.addBlock(pos[0], pos[1], items[0]);
            }
            
        }

        public void LoadAllItems()
        {
            Item pnn;
            // Grass 0
            pnn = new Item();
            pnn.id = 0;
            pnn.name = "Grass";
            pnn.stack_max = 64;
            pnn.states = new List<string>();
            pnn.states.Add("..\\..\\..\\..\\..\\assets\\textures\\Grass.png");
            pnn.curState = 0;
            pnn.quantity = 0;
            pnn.required = new List<Item>();
            pnn.skin = new Bitmap(pnn.states[pnn.curState]);
            pnn.dir = 0;
            pnn.required_quantity = new List<int>();
            pnn.w = 50;
            pnn.h = 50;
            pnn.placementBehavior = "place";
            items.Add(pnn);

            // Egg 1
            pnn = new Item();
            pnn.id = 1;
            pnn.name = "Egg";
            pnn.stack_max = 24;
            pnn.states = new List<string>();
            pnn.states.Add("..\\..\\..\\..\\..\\assets\\items\\egg.png");
            pnn.curState = 0;
            pnn.quantity = 0;
            pnn.required = new List<Item>();
            pnn.skin = new Bitmap(pnn.states[pnn.curState]);
            pnn.dir = 0;
            pnn.required_quantity = new List<int>();
            pnn.w = 50;
            pnn.h = 50;
            pnn.placementBehavior = "throw";
            items.Add(pnn);

            // Stone 2
            pnn = new Item();
            pnn.id = 0;
            pnn.name = "Stone";
            pnn.stack_max = 64;
            pnn.states = new List<string>();
            pnn.states.Add("..\\..\\..\\..\\..\\assets\\textures\\Stone.png");
            pnn.curState = 0;
            pnn.quantity = 0;
            pnn.required = new List<Item>();
            pnn.skin = new Bitmap(pnn.states[pnn.curState]);
            pnn.dir = 0;
            pnn.required_quantity = new List<int>();
            pnn.w = 50;
            pnn.h = 50;
            pnn.placementBehavior = "place";
            items.Add(pnn);

            // Lava 3
            pnn = new Item();
            pnn.id = 0;
            pnn.name = "Lava";
            pnn.stack_max = 64;
            pnn.states = new List<string>();
            pnn.states.Add("..\\..\\..\\..\\..\\assets\\textures\\lava0.png");
            pnn.states.Add("..\\..\\..\\..\\..\\assets\\textures\\lava1.png");
            pnn.curState = 0;
            pnn.quantity = 0;
            pnn.required = new List<Item>();
            pnn.skin = new Bitmap(pnn.states[pnn.curState]);
            pnn.dir = 0;
            pnn.required_quantity = new List<int>();
            pnn.w = 50;
            pnn.h = 50;
            pnn.placementBehavior = "pour";
            items.Add(pnn);

            // Ladder
            pnn = new Item();
            pnn.id = 0;
            pnn.name = "Ladder";
            pnn.stack_max = 64;
            pnn.states = new List<string>();
            pnn.states.Add("..\\..\\..\\..\\..\\assets\\textures\\ladder.png");
            pnn.curState = 0;
            pnn.quantity = 0;
            pnn.required = new List<Item>();
            pnn.skin = new Bitmap(pnn.states[pnn.curState]);
            pnn.dir = 0;
            pnn.required_quantity = new List<int>();
            pnn.w = 50;
            pnn.h = 50;
            pnn.placementBehavior = "place";
            items.Add(pnn);
        }

        private void Game_Paint(object? sender, PaintEventArgs e)
        {
            DrawDubb();
        }

        public void DrawDubb()
        {
            Graphics g2 = Graphics.FromImage(off);
            DrawScene(g2);
            g.DrawImage(off,0,0);
            
        }

        public void DrawScene(Graphics g2)
        {
            g2.Clear(Color.Black);

            float opacity = 0.02f;
            ColorMatrix matrix = new ColorMatrix { Matrix33 = opacity };
            ImageAttributes attributes = new ImageAttributes();
            attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            for (int i=0; i<tiles.grid.Count; i++)
            {
                List<Block?> row = tiles.grid[i];
                for(int j=0; j < row.Count; j++)
                {
                    if (row[j] != null)
                    {
                        g2.DrawImage(row[j].skin, row[j].x - ScrlX, row[j].y - ScrlY, row[j].w, row[j].h);
                        if(effect != null)
                        {
                            for(int k=0; k<effect.x.Count; k++)
                            {
                                g2.DrawImage(effect.skin, new Rectangle(effect.x[k]-ScrlX, effect.y[k]-ScrlY, effect.w, effect.h),0,0,effect.skin.Width, effect.skin.Height, GraphicsUnit.Pixel, attributes);
                            }
                        }
                    }
                    else
                    {
                        if (j * myGraphics.scalar >= ScrlX && j * myGraphics.scalar <= ScrlX + myGraphics.size[0] && i * myGraphics.scalar >= ScrlY && i * myGraphics.scalar <= ScrlY + myGraphics.size[1])
                        {
                            if (weather.skin != null)
                            {
                                Random random = new Random();
                                if (random.Next(0,10) %2 == 0)
                                    g2.DrawImage(weather.skin, j * myGraphics.scalar, i * myGraphics.scalar, myGraphics.scalar, myGraphics.scalar);
                            }
                        }
                    }
                }
            }
            for (int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i].dir < 0)
                {
                    g2.DrawImage(enemies[i].skin, enemies[i].x + enemies[i].w - ScrlX, enemies[i].y - ScrlY, -enemies[i].w, enemies[i].h);
                    if (enemies[i].isOnFire > 0)
                    {
                        g2.DrawImage(new Bitmap("..\\..\\..\\..\\..\\assets\\effect\\flame.png"), enemies[i].x + 3 * enemies[i].w / 4 - ScrlX, (enemies[i].y + enemies[i].h - enemies[i].h / 2) - ScrlY, -enemies[i].w / 2, enemies[i].h / 4);
                    }
                }
                else
                {
                    g2.DrawImage(enemies[i].skin, enemies[i].x - ScrlX, enemies[i].y - ScrlY, enemies[i].w, enemies[i].h);
                    if (enemies[i].isOnFire > 0)
                    {
                        g2.DrawImage(new Bitmap("..\\..\\..\\..\\..\\assets\\effect\\flame.png"), enemies[i].x + enemies[i].w / 4 - ScrlX, (enemies[i].y + enemies[i].h - enemies[i].h / 2) - ScrlY, enemies[i].w / 2, enemies[i].h / 4);
                    }
                }
            }

            for(int i=0; i<ladders.Count; i++)
            {
                //g2.DrawImage(ladders[i].skin, ladders[i].x - ScrlX, ladders[i].y - ScrlY, ladders[i].w, ladders[i].h);
            }

            if (hero.dir < 0)
            {
                g2.DrawImage(hero.skin, hero.x + hero.w - ScrlX, hero.y - ScrlY, -hero.w, hero.h);
                if(hero.isOnFire > 0)
                {
                    g2.DrawImage(new Bitmap("..\\..\\..\\..\\..\\assets\\effect\\flame.png"), hero.x + 3*hero.w/4 - ScrlX, (hero.y + hero.h - hero.h / 2) - ScrlY, -hero.w/2, hero.h / 4);
                }
            }
            else
            {
                g2.DrawImage(hero.skin, hero.x - ScrlX, hero.y - ScrlY, hero.w, hero.h);
                if (hero.isOnFire > 0)
                {
                    g2.DrawImage(new Bitmap("..\\..\\..\\..\\..\\assets\\effect\\flame.png"), hero.x + hero.w / 4 - ScrlX, (hero.y + hero.h - hero.h/2) - ScrlY, hero.w/2, hero.h / 4);
                }
            }

            
                

            for (int i=0; i<lavas.Count; i++)
            {
                for(int j=0; j < lavas[i].path.Count; j++)
                    g2.DrawImage(lavas[i].path[j].skin, lavas[i].path[j].x - ScrlX, lavas[i].path[j].y - ScrlY, lavas[i].path[j].w, lavas[i].path[j].h);
            }

            for(int i=0; i<projectiles.Count; i++)
            {
                g2.DrawImage(projectiles[i].skin, projectiles[i].x-ScrlX, projectiles[i].y-ScrlY, projectiles[i].w, projectiles[i].h);
            }

            for(int i=0; i<hud.elements.Count; i++)
            {
                Element curElement = hud.elements[i];
                if (i < 2)
                {
                    for(int j=0; j<=curElement.cur; j++)
                    {
                        g2.DrawImage(curElement.skins[j], curElement.x[j], curElement.y[j], curElement.w[j], curElement.h[j]);
                    }
                }
                else
                {
                    for (int j = 0; j < curElement.skins.Count; j++)
                    {
                        g2.DrawImage(curElement.skins[j], curElement.x[j], curElement.y[j], curElement.w[j], curElement.h[j]);
                    }
                }
 
                if (i == 2)
                {
                    // display items
                    for (int j = 0; j < hero.hotbar.Count; j++)
                    {
                        g2.DrawImage(hero.hotbar[j].skin, curElement.x[j]+ curElement.w[j]/2- hero.hotbar[j].w/2, curElement.y[j]+ curElement.h[j] / 2 - hero.hotbar[j].h/2, hero.hotbar[j].w, hero.hotbar[j].h);
                        g2.DrawString(hero.hotbar[j].quantity.ToString(), new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0))), new SolidBrush(Color.White), curElement.x[j]+curElement.w[j]-40, curElement.y[j]+ curElement.h[j]-40);
                    }
                }
            }
            g2.FillRectangle(new SolidBrush(Color.FromArgb(80, Color.White)), rushBar.X, rushBar.Y, 1080, 5);
            g2.FillRectangle(new SolidBrush(Color.White), rushBar);
            if(hud.elements[2].cur < hero.hotbar.Count)
                g2.DrawImage(hero.hotbar[hud.elements[2].cur].skin, cursor.x + cursor.radius / 2 - ScrlX - hero.hotbar[hud.elements[2].cur].w / 2, cursor.y + cursor.radius / 2 - ScrlY - hero.hotbar[hud.elements[2].cur].h / 2, hero.hotbar[hud.elements[2].cur].w, hero.hotbar[hud.elements[2].cur].h);
            g2.FillEllipse(new SolidBrush(cursor.color), cursor.x - ScrlX, cursor.y - ScrlY, cursor.radius, cursor.radius);
        }
    }
}
