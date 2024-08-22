using System;
using Godot;

public partial class InformationDisplay: Control
{
    private string[] playerJoinMessages = new string[]
        {
            "{0} has joined the {1} roster!",
            "Welcome aboard, {0}! You're now part of {1}.",
            "{1} just got stronger with {0} joining the squad!",
            "Breaking news: {0} signs with {1}!",
            "{0} dons the {1} gear!",
            "{1} fans, give a warm welcome to your newest teammate, {0}!",
            "{0} is ready to make waves with {1}.",
            "The {1} family grows as {0} comes on board.",
            "Look out! {0} is now playing for {1}.",
            "{1} just leveled up with {0}!"
        };
    private string[] playerDiedMessages = new string[]
        {
            "{0} has fallen in battle for {1}.",
            "{0} met their end while fighting for {1}.",
            "{0} made the ultimate sacrifice for {1}.",
            "A valiant effort, but {0} has perished under {1}'s banner.",
            "The legend of {0} ends here, fighting for {1}.",
            "{1} loses a warrior: {0} is no more.",
            "Rest in peace, {0}. You fought bravely for {1}.",
            "The battlefield claims {0}, loyal to {1}.",
            "A tragic loss for {1}: {0} has fallen.",
            "The fight is over for {0}, who served {1} till the end."
        };
    // 0 = displayName, 1 = teamAbrrev, 2 = distance
    private string[] playerFaceplantMessages = new string[]
    {
        "Ouch! {1} {0} ate dirt.",
        "How was the {2}m fall, {1} {0}?",
        "{0} from {1} just kissed the ground after {2}m!",
        "{1}'s {0} hit the ground hard after {2}m!",
        "Gravity wins! {0} from {1} faceplants after {2}m.",
        "That's gonna leave a mark! {0} of {1} lands face-first after {2}m.",
        "{0} of {1} just invented a new move: the {2}m faceplant!",
        "Hope {1} {0} likes the taste of dirt after that {2}m dive.",
        "That was a rough landing! {1}'s {0} faceplants after {2}m.",
        "{1}'s {0} went for a {2}m dive and found the ground the hard way!"
    };
    // 0 - displayName, 1 = teamAbbrev, 2 = combo
    private string[] playerComboStreakMessages = new string[]
    {
        "ZoowieMAMA {1} {0} is here with a {2} combo.",
        "{1} {0} back at it again with that {2} streak.",
        "C-C-Combo breaker! Nice {2} combo {0}.",
        "Zoinks! {1} is gaining points with {0}'s performance.",
        "{0} from {1} is defying gravity with a {2}-platform streak!",
        "Unbelievable! {0} of {1} just conquered {2} platforms in a row!",
        "{1}'s {0} is on a jumping spree with a {2}-platform combo!",
        "Impressive! {0} from {1} just leaped to a {2}-platform streak!",
        "Like a pro, {0} from {1} lands a flawless {2}-platform combo!",
        "No stopping now! {1}'s {0} is on a {2}-platform rampage!",
        "{1}'s {0} is on a hot streak, hitting {2} platforms consecutively!",
        "{0} from {1} is showing off with a seamless {2}-platform combo!"
    };
}