using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XnaCards;

namespace ProgrammingAssignment6
{
    /// <summary>
    /// This is the main type for the game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        const int WindowWidth = 800;
        const int WindowHeight = 600;

        // max valid blockjuck score for a hand
        const int MaxHandValue = 21;
        const int MaxDealerHitValue = 19;

        // deck and hands
        Deck deck;
        List<Card> dealerHand = new List<Card>();
        List<Card> playerHand = new List<Card>();

        // hand placement
        const int TopCardOffset = 100;
        const int HorizontalCardOffset = 150;
        const int VerticalCardSpacing = 125;
        int numberOfCardsPlayerHand;
        int numberOfCardsDealerHand;
        // messages
        SpriteFont messageFont;
        const string ScoreMessagePrefix = "Score: ";
        Message playerScoreMessage;
        Message dealerScoreMessage;
        Message winnerMessage;
        string winnerText;
        
		List<Message> messages = new List<Message>();

        // message placement
        const int ScoreMessageTopOffset = 25;
        const int HorizontalMessageOffset = HorizontalCardOffset;
        Vector2 winnerMessageLocation = new Vector2(WindowWidth / 2,
            WindowHeight / 2);

        // menu buttons
        Texture2D quitButtonSprite;
        List<MenuButton> menuButtons = new List<MenuButton>();

        // menu button placement
        const int TopMenuButtonOffset = TopCardOffset;
        const int QuitMenuButtonOffset = WindowHeight - TopCardOffset;
        const int HorizontalMenuButtonOffset = WindowWidth / 2;
        const int VerticalMenuButtonSpacing = 125;

        // use to detect hand over when player and dealer didn't hit
        bool playerHit = false;
        bool dealerHit = false;

        // use to detect if player or dealer are done with the game
        bool dealerDone = false;
        bool playerDone = false;

        // game state tracking
        static GameState currentState = GameState.WaitingForPlayer;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // set resolution and show mouse
            graphics.PreferredBackBufferWidth = WindowWidth;
            graphics.PreferredBackBufferHeight = WindowHeight;
            IsMouseVisible = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // create and shuffle deck
            deck = new Deck(Content, 0, 0);
            deck.Shuffle();

            // first player card
            Card playerCard0 = deck.TakeTopCard();
            playerCard0.FlipOver();
            playerCard0.X = HorizontalCardOffset;
            playerCard0.Y = TopCardOffset;            
            playerHand.Add(playerCard0);

            // first dealer card
            Card dealerCard0 = deck.TakeTopCard();            
            dealerCard0.X = WindowWidth - HorizontalCardOffset;
            dealerCard0.Y = TopCardOffset;
            dealerHand.Add(dealerCard0);

            // second player card            
            Card playerCard1 = deck.TakeTopCard();
            playerCard1.FlipOver();
            playerCard1.X = HorizontalCardOffset;
            playerCard1.Y = TopCardOffset + VerticalCardSpacing;
            playerHand.Add(playerCard1);

            // second dealer card
            Card dealerCard1 = deck.TakeTopCard();
            dealerCard1.FlipOver();
            dealerCard1.X = WindowWidth - HorizontalCardOffset;
            dealerCard1.Y = TopCardOffset + VerticalCardSpacing;
            dealerHand.Add(dealerCard1);

            // load sprite font, create message for player score and add to list
            messageFont = Content.Load<SpriteFont>(@"fonts\Arial24");
            playerScoreMessage = new Message(ScoreMessagePrefix + GetBlockjuckScore(playerHand).ToString(),
                messageFont,
                new Vector2(HorizontalMessageOffset, ScoreMessageTopOffset));
            messages.Add(playerScoreMessage);

            // load quit button sprite for later use
			quitButtonSprite = Content.Load<Texture2D>(@"graphics\quitbutton");

            // create hit button and add to list
            MenuButton hitButton = new MenuButton(Content.Load<Texture2D>(@"graphics\hitbutton"), 
                new Vector2(HorizontalMenuButtonOffset, TopMenuButtonOffset), 
                GameState.PlayerHitting);
            menuButtons.Add(hitButton);

            // create stand button and add to list
            MenuButton standButton = new MenuButton(Content.Load<Texture2D>(@"graphics\standbutton"),
                new Vector2(HorizontalMenuButtonOffset, TopMenuButtonOffset + VerticalMenuButtonSpacing),
                GameState.PlayerPassing);
            menuButtons.Add(standButton);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();


            // update menu buttons as appropriate

            MouseState mouse = Mouse.GetState();
            foreach (MenuButton button in menuButtons)
            {
                if (currentState == GameState.WaitingForPlayer || currentState == GameState.DisplayingHandResults)
                {
                    button.Update(mouse);
                }

            }


            // game state-specific processing
            
            switch (currentState)
            {
                // case if player hitting
                case (GameState.PlayerHitting):
                    numberOfCardsPlayerHand = playerHand.Count;
                    Card playerCard2 = deck.TakeTopCard();
                    playerCard2.FlipOver();
                    playerCard2.X = HorizontalCardOffset;
                    playerCard2.Y = TopCardOffset + numberOfCardsPlayerHand * VerticalCardSpacing;
                    playerHand.Add(playerCard2);
                    playerScoreMessage.Text = ScoreMessagePrefix + GetBlockjuckScore(playerHand).ToString();
                    playerHit = true;
                    Game1.ChangeState(GameState.WaitingForDealer);
                    break;

                // case of player is passing
                case (GameState.PlayerPassing):
                    playerDone = true;
                    if (dealerDone)
                    {                        
                        Game1.ChangeState(GameState.CheckingHandOver);
                    }
                    else
                    {
                        Game1.ChangeState(GameState.WaitingForDealer);
                    }                        
                    break;

                // case if waiting for dealer
                case (GameState.WaitingForDealer):

                    // dealer decide to hit
                    if (GetBlockjuckScore(dealerHand) < MaxDealerHitValue)
                    {
                        Game1.ChangeState(GameState.DealerHitting);
                    }

                    // dealer decide to stand or he got 21
                    else 
                    {
                        dealerHit = false;
                        dealerDone = true;
                        Game1.ChangeState(GameState.CheckingHandOver);
                    }
                    break;

                // case if dealer hitting
                case (GameState.DealerHitting):

                    numberOfCardsDealerHand = dealerHand.Count;
                    Card dealerCard2 = deck.TakeTopCard();
                    dealerCard2.FlipOver();
                    dealerCard2.X = WindowWidth - HorizontalCardOffset;
                    dealerCard2.Y = TopCardOffset + numberOfCardsDealerHand * VerticalCardSpacing;
                    dealerHand.Add(dealerCard2);
                    dealerHit = true;
                    if (GetBlockjuckScore(dealerHand) >= MaxDealerHitValue && GetBlockjuckScore(dealerHand) <= MaxHandValue)
                        dealerDone = true;
                    else if (GetBlockjuckScore(dealerHand) > MaxHandValue)
                    {
                        dealerDone = true;
                        playerDone = true;
                    }
                    Game1.ChangeState(GameState.CheckingHandOver);
                    break;

                // case checking hand over

                case (GameState.CheckingHandOver):

                    if (!playerDone && GetBlockjuckScore(playerHand) <= MaxHandValue)
                    {
                        dealerHit = false;
                        playerHit = false;
                        Game1.ChangeState(GameState.WaitingForPlayer);
                    }
                    else if (playerDone && !dealerDone)
                    {
                        dealerHit = false;
                        playerHit = false;
                        Game1.ChangeState(GameState.WaitingForDealer);
                    }
                    else 
                    { 
                       if (GetBlockjuckScore(playerHand) <= MaxHandValue && 
                            (GetBlockjuckScore(dealerHand) > MaxHandValue ||                           
                           dealerDone && playerDone && GetBlockjuckScore(playerHand) > GetBlockjuckScore(dealerHand)))
                        {
                            winnerText = "YOU WON!";
                            winnerMessage = new Message(winnerText, messageFont, winnerMessageLocation);
                            messages.Add(winnerMessage);
                        }
                        else if (GetBlockjuckScore(dealerHand) <= MaxHandValue &&
                            (GetBlockjuckScore(playerHand) > MaxHandValue ||
                           dealerDone && playerDone &&
                           GetBlockjuckScore(playerHand) < GetBlockjuckScore(dealerHand)))
                        {
                            winnerText = "DEALER WON!";
                            winnerMessage = new Message(winnerText, messageFont, winnerMessageLocation);
                            messages.Add(winnerMessage);
                        }
                        else if (GetBlockjuckScore(dealerHand) > MaxHandValue &&
                           GetBlockjuckScore(playerHand) > MaxHandValue ||
                           dealerDone && playerDone &&
                           GetBlockjuckScore(dealerHand) == GetBlockjuckScore(playerHand))
                        {
                            winnerText = "TIE!";
                            winnerMessage = new Message(winnerText, messageFont, winnerMessageLocation);
                            messages.Add(winnerMessage);
                        }
                        
                        dealerHand[0].FlipOver();
                        dealerScoreMessage = new Message(ScoreMessagePrefix + GetBlockjuckScore(dealerHand).ToString(),
                            messageFont,
                    new Vector2(WindowWidth - HorizontalMessageOffset, ScoreMessageTopOffset));
                        messages.Add(dealerScoreMessage);
                        for (int i = 0; i < menuButtons.Count; i++)
                        {
                            menuButtons.RemoveAt(i);
                        }
                        MenuButton exitButton0 = new MenuButton(quitButtonSprite,
                            new Vector2(HorizontalMenuButtonOffset, TopMenuButtonOffset + VerticalMenuButtonSpacing),
                    GameState.Exiting);
                        menuButtons.Add(exitButton0);
                        Game1.ChangeState(GameState.DisplayingHandResults);
                    }         
                    break;

                // case for exit
                case (GameState.Exiting):

                    this.Exit();
                    break;


            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Goldenrod);
						
            spriteBatch.Begin();

            // draw hands
            foreach (Card dealerCards in dealerHand)
            {
                dealerCards.Draw(spriteBatch);
            }

            foreach (Card playerCards in playerHand)
            {
                playerCards.Draw(spriteBatch);
            }

            // draw messages
            foreach (Message message in messages)
            {
                message.Draw(spriteBatch);
            }

            

            // draw menu buttons
            foreach (MenuButton button in menuButtons)
            {
                button.Draw(spriteBatch);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Calculates the Blockjuck score for the given hand
        /// </summary>
        /// <param name="hand">the hand</param>
        /// <returns>the Blockjuck score for the hand</returns>
        private int GetBlockjuckScore(List<Card> hand)
        {
            // add up score excluding Aces
            int numAces = 0;
            int score = 0;
            foreach (Card card in hand)
            {
                if (card.Rank != Rank.Ace)
                {
                    score += GetBlockjuckCardValue(card);
                }
                else
                {
                    numAces++;
                }
            }

            // if more than one ace, only one should ever be counted as 11
            if (numAces > 1)
            {
                // make all but the first ace count as 1
                score += numAces - 1;
                numAces = 1;
            }

            // if there's an Ace, score it the best way possible
            if (numAces > 0)
            {
                if (score + 11 <= MaxHandValue)
                {
                    // counting Ace as 11 doesn't bust
                    score += 11;
                }
                else
                {
                    // count Ace as 1
                    score++;
                }
            }

            return score;
        }

        /// <summary>
        /// Gets the Blockjuck value for the given card
        /// </summary>
        /// <param name="card">the card</param>
        /// <returns>the Blockjuck value for the card</returns>
        private int GetBlockjuckCardValue(Card card)
        {
            switch (card.Rank)
            {
                case Rank.Ace:
                    return 11;
                case Rank.King:
                case Rank.Queen:
                case Rank.Jack:
                case Rank.Ten:
                    return 10;
                case Rank.Nine:
                    return 9;
                case Rank.Eight:
                    return 8;
                case Rank.Seven:
                    return 7;
                case Rank.Six:
                    return 6;
                case Rank.Five:
                    return 5;
                case Rank.Four:
                    return 4;
                case Rank.Three:
                    return 3;
                case Rank.Two:
                    return 2;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Changes the state of the game
        /// </summary>
        /// <param name="newState">the new game state</param>
        public static void ChangeState(GameState newState)
        {
            currentState = newState;
        }
    }
}
