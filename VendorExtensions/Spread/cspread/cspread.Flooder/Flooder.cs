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
using spread;
public class Flooder {
	public Flooder(String user, int numMessages, int numBytes, String address, int port, bool readOnly, bool writeOnly) {
		try {
			// Start timer.
			///////////////
			DateTime startTime = DateTime.Now;
			
			// Connect.
			///////////
			SpreadConnection connection = new SpreadConnection();
			connection.Connect(address, port, user, false, false);
			string privateName = connection.PrivateGroup.ToString();
			
			// Join.
			////////
			SpreadGroup group = new SpreadGroup();
			if(readOnly) {
				Console.WriteLine("Only receiving messages");
				group.Join(connection, "flooder");
			}
			else if(writeOnly) {
				Console.WriteLine("Starting multicast of " + numMessages + " messages, " + numBytes + " bytes each (self discarding).");
			}
			else {
				group.Join(connection, "flooder");
				Console.WriteLine("Starting multicast of " + numMessages + " messages, " + numBytes + " bytes each.");
			}
			
			// The outgoing message.
			////////////////////////
			SpreadMessage mout = null;
			if(readOnly == false) {
				mout = new SpreadMessage();
				mout.IsSafe = true;
				mout.Data = new byte[numBytes];
				mout.AddGroup("flooder");
			}
			
			// Send/Receive.
			////////////////
			for(int i = 1 ; i <= numMessages ; i++) {
				// Send.
				////////
				if(readOnly == false) {
					connection.Multicast(mout);
					}
				
				// Receive.
				///////////
				if((readOnly) || ((i > 50) && (writeOnly == false))) {
					SpreadMessage min;
					do {
						min = connection.Receive();
					}
					while((readOnly == false) && (privateName.Equals(min.Sender.ToString()) == false));
				}
				
				// Report.
				//////////
				if((i % 1000) == 0) {
					Console.WriteLine("Completed " + i + " messages");
				}
			}
			
			// Stop timer.
			//////////////
			DateTime stopTime = DateTime.Now;
			TimeSpan time = stopTime.Subtract(startTime);
			double Mbps = numBytes;
			Mbps *= numMessages;
			Mbps *= 8;
			if((readOnly == false) && (writeOnly == false))
				Mbps *= 2;
			Mbps *= 1000;
			Mbps /= time.TotalMilliseconds;
			Mbps /= (1024 * 1024);
			Console.WriteLine("Time: " + time + "ms (" + (int)Mbps + "." + (((int)(Mbps * 100)) % 100) + " Mbps)");
		}
		catch(Exception e) {
			Console.WriteLine(e);
		}
	}
	
	public static void Main(String[] args) {
		String user = "Flooder";
		int numMessages = 10000;
		int numBytes = 1000;
		String address = null;
		int port = 0;
		bool readOnly = false;
		bool writeOnly = false;
		
		for(int i = 0 ; i < args.Length ; i++) {
			// Check for user.
			//////////////////
			if((args[i].CompareTo("-u") == 0) && (args.Length > (i + 1))) {
				// Set user.
				////////////
				i++;
				user = args[i];
			}
				// Check for numMessages.
				/////////////////////////
			else if((args[i].CompareTo("-m") == 0) && (args.Length > (i + 1))) {
				// Set numMessages.
				///////////////////
				i++;
				numMessages = int.Parse(args[i]);
			}
				// Check for numBytes.
				//////////////////////
			else if((args[i].CompareTo("-b") == 0) && (args.Length > (i + 1))) {
				// Set numBytes.
				////////////////
				i++;
				numBytes = int.Parse(args[i]);
			}
				// Check for address.
				/////////////////////
			else if((args[i].CompareTo("-s") == 0) && (args.Length > (i + 1))) {
				// Set address.
				///////////////
				i++;
				address = args[i];
			}
				// Check for port.
				//////////////////
			else if((args[i].CompareTo("-p") == 0) && (args.Length > (i + 1))) {
				// Set port.
				////////////
				i++;
				port = int.Parse(args[i]);
			}
				// Check for readOnly.
				//////////////////////
			else if(args[i].CompareTo("-ro") == 0) {
				// Set readOnly.
				////////////////
				readOnly = true;
				writeOnly = false;
			}
				// Check for writeOnly.
				///////////////////////
			else if(args[i].CompareTo("-wo") == 0) {
				// Set writeOnly.
				/////////////////
				writeOnly = true;
				readOnly = false;
			}
			else {
				Console.Write("Usage: flooder\n" + 
						   "\t[-u <user name>]     : unique user name\n" + 
						   "\t[-m <num messages>]  : number of messages\n" + 
						   "\t[-b <num bytes>]     : number of bytes per message\n" + 
						   "\t[-s <address>]       : the name or IP for the daemon\n" + 
						   "\t[-p <port>]          : the port for the daemon\n" + 
						   "\t[-ro]                : read  only (no multicast)\n" + 
						   "\t[-wo]                : write only (no receive)\n");
				Environment.Exit(0);
			}
		}
		
		Flooder f = new Flooder(user, numMessages, numBytes, address, port, readOnly, writeOnly);
	}
}
