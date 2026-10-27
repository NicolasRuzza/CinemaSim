Feature: Hold a seat
  As a customer at a self-service kiosk
  I want to temporarily hold a seat
  So that nobody else can take it while I finish the purchase

  Background:
    Given a session "S1" with the seats "A1" and "A2" available
    And a session "S2" with the seat "A1" available

  Scenario: Hold an available seat
    When customer "Bob" holds seat "A1" of session "S1"
    Then the hold is accepted
    And the hold returns a reservation ID

  Scenario: Accept a repeated hold by the same customer
    Given customer "Bob" holds seat "A1" of session "S1"
    And the hold returned a reservation ID
    When customer "Bob" holds seat "A1" of session "S1"
    Then the hold is accepted
    And the hold returns the same reservation ID

  Scenario: Reject a hold on a seat already held by another customer
    Given customer "Bob" holds seat "A1" of session "S1"
    When customer "Jessica" holds seat "A1" of session "S1"
    Then the hold is rejected because the seat is already held

  @concurrency
  Scenario: Only one of several simultaneous holds is accepted
    When 10 different customers try to hold seat "A1" of session "S1" at the same time
    Then exactly 1 hold is accepted
    And the other 9 holds are rejected because the seat is already held

  Scenario: Holding a seat does not block a different seat of the same session
    Given customer "Jessica" holds seat "A1" of session "S1"
    When customer "Bob" holds seat "A2" of session "S1"
    Then the hold is accepted

  Scenario: Holding a seat does not block the same seat in another session
    Given customer "Jessica" holds seat "A1" of session "S1"
    When customer "Bob" holds seat "A1" of session "S2"
    Then the hold is accepted

  Scenario: A hold is still active just before 5 minutes
    Given customer "Bob" holds seat "A1" of session "S1"
    And 4 minutes and 59 seconds have passed
    When customer "Jessica" holds seat "A1" of session "S1"
    Then the hold is rejected because the seat is already held

  Scenario: A hold expires after 5 minutes
    Given customer "Bob" holds seat "A1" of session "S1"
    And 5 minutes have passed
    When customer "Jessica" holds seat "A1" of session "S1"
    Then the hold is accepted

  Scenario: A hold made after the previous one expired gets a new reservation ID
    Given customer "Bob" holds seat "A1" of session "S1"
    And the hold returned a reservation ID
    And 5 minutes have passed
    When customer "Bob" holds seat "A1" of session "S1"
    Then the hold is accepted
    And the hold returns a different reservation ID

  Scenario: A repeated hold does not extend the 5-minute duration
    Given customer "Bob" holds seat "A1" of session "S1"
    And 3 minutes have passed
    And customer "Bob" holds seat "A1" of session "S1"
    And 2 minutes have passed
    When customer "Jessica" holds seat "A1" of session "S1"
    Then the hold is accepted

  Scenario: Reject a hold on a nonexistent session
    When customer "Bob" holds seat "A1" of session "S9"
    Then the hold is rejected because the session does not exist

  Scenario: Reject a hold on a nonexistent seat
    When customer "Bob" holds seat "Z9" of session "S1"
    Then the hold is rejected because the seat does not exist
