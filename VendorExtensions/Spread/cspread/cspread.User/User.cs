/*
 * The Spread Toolkit.
 *     
 * The contents of this file are subject to the Spread Open-Source
 * License, Version 1.0 (the ``License''); you may not use
 * this file except in compliance with the License.  You may obtain a
 * copy of the License at:
 *
 * http://www.spread.org/license/
 *
 * or in the file ``license.txt'' found in this distribution.
 *
 * Software distributed under the License is distributed on an AS IS basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License 
 * for the specific language governing rights and limitations under the 
 * License.
 *
 * The Creators of Spread are:
 *  Yair Amir, Michal Miskin-Amir, Jonathan Stanton.
 *
 *  Copyright (C) 1993-2001 Spread Concepts LLC <spread@spreadconcepts.com>
 *
 *  All Rights Reserved.
 *
 * Major Contributor(s):
 * ---------------
 *    Dan Schoenblum   dansch@cnds.jhu.edu - Java Interface Developer.
 *    John Schultz     jschultz@cnds.jhu.edu - contribution to process group membership.
 *    Theo Schlossnagle theos@cnds.jhu.edu - Perl library and Skiplists.
 *
 */
using System;
using System.Threading;
using spread;
public class User {
	// The Spread Connection.
	/////////////////////////
	private SpreadConnection connection;
	
	// A group.
	///////////
	private SpreadGroup group;
	
	// The number of messages sent.
	///////////////////////////////
	private int numSent;
	
	// True if there is a listening thread.
	///////////////////////////////////////
	private bool listening;
	private recThread rt;
	private SpreadConnection.MessageHandler handler;

	private void PrintMenu() {
		// Print the menu.
		//////////////////
		Console.Write("\n" +
			"==========\n" +
			"User Menu:\n" +
			"==========\n" +
			"\n" +
			"\tj <group> -- join a group\n" + 
			"\tl -- leave a group\n" +
			"\n" +
			"\ts <group> -- send a message\n" +
			"\tb <group> -- send a burst of messages\n" + 
			"\n");
		if(listening == false) {
			Console.Write("\tr -- receive a message\n" +
				"\tp -- poll for a message\n" +
				"\tt -- add a listener\n" + 
				"\n");
		}
		else {
			Console.Write("\tt -- stop the listener\n" +
				"\n");
		}
		Console.Write("\tq -- quit\n");
	}
	
	private void DisplayMessage(SpreadMessage msg) {
		try {
			if(msg.IsRegular) {
				Console.Write("Received a ");
				if(msg.IsUnreliable)
					Console.Write("UNRELIABLE");
				else if(msg.IsReliable)
					Console.Write("RELIABLE");
				else if(msg.IsFifo)
					Console.Write("FIFO");
				else if(msg.IsCausal)
					Console.Write("CAUSAL");
				else if(msg.IsAgreed)
					Console.Write("AGREED");
				else if(msg.IsSafe)
					Console.Write("SAFE");
				Console.WriteLine(" message.");
				
				Console.WriteLine("Sent by  " + msg.Sender + ".");
				
				Console.WriteLine("Type is " + msg.Type + ".");
				
				if(msg.EndianMismatch == true)
					Console.WriteLine("There is an endian mismatch.");
				else
					Console.WriteLine("There is no endian mismatch.");
				
				SpreadGroup[] groups = msg.Groups;
				Console.WriteLine("To " + groups.Length + " groups.");
				
				byte[] data = msg.Data;
				Console.WriteLine("The data is " + data.Length + " bytes.");
				
				Console.WriteLine("The message is: " + System.Text.Encoding.ASCII.GetString(data));
			}
			else if (msg.IsMembership) {
				MembershipInfo info = msg.MembershipInfo;
				
				if(info.IsRegularMembership) {
					SpreadGroup[] groups = msg.Groups;
				
					Console.WriteLine("Received a REGULAR membership.");
					Console.WriteLine("For group " + info.Group + ".");
					Console.WriteLine("With " + groups.Length + " members.");
					Console.WriteLine("I am member " + msg.Type + ".");
					for(int i = 0 ; i < groups.Length ; i++)
						Console.WriteLine("  " + groups[i]);
				
					Console.WriteLine("Group ID is " + info.GroupID);
				
					Console.Write("Due to ");
					if(info.IsCausedByJoin) {
						Console.WriteLine("the JOIN of " + info.Joined + ".");
					}
					else if(info.IsCausedByLeave) {
						Console.WriteLine("the LEAVE of " + info.Left + ".");
					}
					else if(info.IsCausedByDisconnect) {
						Console.WriteLine("the DISCONNECT of " + info.Disconnected + ".");
					}
					else if(info.IsCausedByNetwork) {
						SpreadGroup[] stayed = info.Stayed;
						Console.WriteLine("NETWORK change.");
						Console.WriteLine("VS set has " + stayed.Length + " members:");
						for(int i = 0 ; i < stayed.Length ; i++)
							Console.WriteLine("  " + stayed[i]);
					}
				}
				else if(info.IsTransition) {
					Console.WriteLine("Received a TRANSITIONAL membership for group " + info.Group);
				}
				else if(info.IsSelfLeave) {
					Console.WriteLine("Received a SELF-LEAVE message for group " + info.Group);
				}
			} else if ( msg.IsReject ) {
				// Received a Reject message 
				Console.Write("Received a ");
				if(msg.IsUnreliable)
					Console.Write("UNRELIABLE");
				else if(msg.IsReliable)
					Console.Write("RELIABLE");
				else if(msg.IsFifo)
					Console.Write("FIFO");
				else if(msg.IsCausal)
					Console.Write("CAUSAL");
				else if(msg.IsAgreed)
					Console.Write("AGREED");
				else if(msg.IsSafe)
					Console.Write("SAFE");
				Console.WriteLine(" REJECTED message.");
				
				Console.WriteLine("Sent by  " + msg.Sender + ".");
				
				Console.WriteLine("Type is " + msg.Type + ".");
				
				if(msg.EndianMismatch == true)
					Console.WriteLine("There is an endian mismatch.");
				else
					Console.WriteLine("There is no endian mismatch.");
				
				SpreadGroup[] groups = msg.Groups;
				Console.WriteLine("To " + groups.Length + " groups.");
				
				byte[] data = msg.Data;
				Console.WriteLine("The data is " + data.Length + " bytes.");
				
				Console.WriteLine("The message is: " + System.Text.Encoding.ASCII.GetString(data));
			} else {
				Console.WriteLine("Message is of unknown type: " + msg.ServiceType );
			}
		}
		catch(Exception e) {
			Console.WriteLine(e);
			Environment.Exit(1);
		}
	}
	
	private void UserCommand() {
		// Show the prompt.
		///////////////////
		Console.Write("\n" + 
			"User> ");
		
		// Get the input.
		/////////////////
		string[] tokens = Console.ReadLine().Split(new Char[] {' '});
		
		// Check what it is.
		////////////////////
		SpreadMessage msg;
		char command = tokens[0].Length>0?tokens[0][0]:'\n';
		try {
			switch(command) {
					//JOIN
				case 'j':
					// Join the group.
					//////////////////
					if(tokens.Length > 1) {
						group = new SpreadGroup();
						group.Join(connection, tokens[1]);
						Console.WriteLine("Joined " + group + ".");
					}
					else {
						System.Console.Error.WriteLine("No group name.");
					}
				
					break;
				
					//LEAVE
				case 'l':
					// Leave the group.
					///////////////////
					if(group != null) {
						group.Leave();
						Console.WriteLine("Left " + group + ".");
					}
					else {
						Console.WriteLine("No group to leave.");
					}
				
					break;
				
					//SEND
				case 's':
					// Get a new outgoing message.
					//////////////////////////////
					msg = new SpreadMessage();
					msg.IsSafe = true;
				
					// Add the groups.
					//////////////////
					for(int i=1;i < tokens.Length;i++)
						msg.AddGroup(tokens[i]);
				
					// Get the message.
					///////////////////
					Console.Write("Enter message: ");
					msg.Data = System.Text.Encoding.ASCII.GetBytes(Console.ReadLine());
				
					// Send it.
					///////////
					connection.Multicast(msg);
				
					// Increment the sent message count.
					////////////////////////////////////
					numSent++;
				
					// Show how many were sent.
					///////////////////////////
					Console.WriteLine("Sent message " + numSent + ".");
				
					break;
				
					//BURST
				case 'b':				
					// Get a new outgoing message.
					//////////////////////////////
					msg = new SpreadMessage();
					msg.IsSafe = true;;
				
					// Get the group.
					/////////////////
					if(tokens.Length > 1) {
						msg.AddGroup(tokens[1]);
					}
					else {
						Console.Error.WriteLine("No group name.");
						break;
					}
				
					// Get the message size.
					////////////////////////
					Console.Write("Enter the size of each message: ");
					int size = int.Parse(Console.ReadLine());
					if(size < 0)
						size = 20;
				
					// Send the messages.
					/////////////////////
					Console.WriteLine("Sending 10 messages of " + size + " bytes.");
					byte[] data = new byte[size];
					for(int i = 0 ; i < size ; i++) {
						data[i] = 0;
					}
					for(int i = 0 ; i < 10 ; i++) {
						// Increment the sent message count.
						////////////////////////////////////
						numSent++;
					
						// Set the message data.
						////////////////////////
						byte[] mess = System.Text.Encoding.ASCII.GetBytes("mess num " + i);
						Array.Copy(mess, 0, data, 0, mess.Length);
						msg.Data = data;
					
						// Send the message.
						////////////////////
						connection.Multicast(msg);
						Console.WriteLine("Sent message " + (i + 1) + " (total " + numSent + ").");
					}
				
					break;
				
					//RECEIVE
				case 'r':
					if(listening == false) {
						// Receive a message.
						/////////////////////
						DisplayMessage(connection.Receive());
					}
				
					break;
				
					//POLL
				case 'p':
					if(listening == false) {
						// Poll.
						////////
						if(connection.Poll() == true) {
							Console.WriteLine("There is a message waiting.");
						}
						else {
							Console.WriteLine("There is no message waiting.");
						}
					}
				
					break;
				
					//THREAD
				case 't':
					if(listening) {
						connection.OnRegularMessage -= handler;
						connection.OnMembershipMessage -= handler;
						if (rt.threadSuspended)
							lock(rt) {
								Monitor.Pulse(rt);
								rt.threadSuspended = false;
							}
					}
					else {
						connection.OnRegularMessage += handler;
						connection.OnMembershipMessage += handler;
						lock(rt) {
							rt.threadSuspended = true;
						}
					}
				
					listening = !listening;

					break;

					//QUIT
				case 'q':
					// Disconnect.
					//////////////
					connection.Disconnect();
				
					// Quit.
					////////
					Environment.Exit(0);
				
					break;
				
				default:
					// Unknown command.
					///////////////////
					Console.WriteLine("Unknown command");
				
					// Show the menu again.
					///////////////////////
					PrintMenu();
					break;
			}
		}
		catch(Exception e) {
			Console.WriteLine(e);
			Environment.Exit(1);
		}
	}
	
	public User(String user, String address, int port) {
		handler = new SpreadConnection.MessageHandler(messageReceived);
		
		// Establish the spread connection.
		///////////////////////////////////
		try {
			connection = new SpreadConnection();
			connection.Connect(address, port, user, false, true);
		}
		catch(SpreadException e) {
			Console.Error.WriteLine("There was an error connecting to the daemon.");
			Console.WriteLine(e);
			Environment.Exit(1);
		}
		catch(Exception e) {
			Console.Error.WriteLine("Can't find the daemon " + address);
			Console.WriteLine(e);
			Environment.Exit(1);
		}

		rt = new recThread(connection);
		Thread rtt = new Thread(new ThreadStart(rt.run));
		rtt.Start();
		// Show the menu.
		/////////////////
		PrintMenu();
		
		// Get a user command.
		//////////////////////
		while(true) {
			UserCommand();
		}
	}

	public void messageReceived(SpreadMessage message) {
		DisplayMessage(message);
	}
	
	public static void Main(String[] args) {			
					 // Default values.
					 //////////////////
					 String user = "User";
					 String address = null;
					 int port = 0;
		
					 // Check the args.
					 //////////////////
					 for(int i = 0 ; i < args.Length ; i++) {
						 // Check for user.
						 //////////////////
						 if((args[i].CompareTo("-u") == 0) && (args.Length > (i + 1))) {
							 // Set user.
							 ////////////
							 i++;
							 user = args[i];
						 }
							 // Check for server.
							 ////////////////////
						 else if((args[i].CompareTo("-s") == 0) && (args.Length > (i + 1))) {
							 // Set the server.
							 //////////////////
							 i++;
							 address = args[i];
						 }
							 // Check for port.
							 //////////////////
						 else if((args[i].CompareTo("-p") == 0) && (args.Length > (i + 1))) {
							 // Set the port.
							 ////////////////
							 i++;
							 port = int.Parse(args[i]);
						 }
						 else {
							 Console.Write("Usage: user\n" + 
										"\t[-u <user name>]   : unique user name\n" + 
										"\t[-s <address>]     : the name or IP for the daemon\n" + 
										"\t[-p <port>]        : the port for the daemon\n");
							 Environment.Exit(0);
						 }
					 }
		
					 User u = new User(user, address, port);
				 }
}
